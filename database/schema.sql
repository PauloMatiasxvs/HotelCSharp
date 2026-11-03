-- Banco do HotelCSharp (PostgreSQL / Supabase).
-- Rode no SQL Editor do Supabase para criar as tabelas num projeto novo.

create extension if not exists btree_gist with schema extensions;

create table public.hospedes (
  id integer generated always as identity primary key,
  nome text not null check (length(trim(nome)) > 0),
  cpf char(11) not null unique check (cpf ~ '^[0-9]{11}$'),
  telefone text,
  email text,
  criado_em timestamptz not null default now()
);

create table public.quartos (
  id integer generated always as identity primary key,
  numero integer not null unique check (numero > 0),
  andar integer not null check (andar >= 0),
  tipo text not null check (tipo in ('STANDARD', 'LUXO', 'SUITE'))
);

create table public.reservas (
  id integer generated always as identity primary key,
  hospede_id integer not null references public.hospedes (id),
  quarto_id integer not null references public.quartos (id),
  entrada date not null,
  saida date not null,
  quantidade_hospedes integer not null check (quantidade_hospedes > 0),
  status text not null check (status in ('CONFIRMADA', 'CHECKIN', 'FINALIZADA', 'CANCELADA')),
  valor_total numeric(10, 2) not null check (valor_total >= 0),
  criado_em timestamptz not null default now(),
  check (saida > entrada),
  -- A mesma regra do C#, garantida também pelo banco:
  -- um quarto não pode ter duas reservas ativas com datas que se cruzam.
  constraint reservas_sem_conflito exclude using gist (
    quarto_id with =,
    daterange(entrada, saida, '[)') with &&
  ) where (status in ('CONFIRMADA', 'CHECKIN'))
);

create index reservas_hospede_id_idx on public.reservas (hospede_id);

-- Acesso pela API com a chave publishable (projeto de estudo, sem login de usuário).
alter table public.hospedes enable row level security;
alter table public.quartos enable row level security;
alter table public.reservas enable row level security;

create policy "acesso_demo_hospedes" on public.hospedes for all to anon using (true) with check (true);
create policy "acesso_demo_quartos" on public.quartos for all to anon using (true) with check (true);
create policy "acesso_demo_reservas" on public.reservas for all to anon using (true) with check (true);
