-- ========== DROP ALT (barn før forælder) ==========
DROP TABLE IF EXISTS order_items;
DROP TABLE IF EXISTS orders;
DROP TABLE IF EXISTS user_challenges;
DROP TABLE IF EXISTS challenges;
DROP TABLE IF EXISTS hydration_logs;
DROP TABLE IF EXISTS hydration_plans;
DROP TABLE IF EXISTS running_logs;
DROP TABLE IF EXISTS running_plans;
DROP TABLE IF EXISTS sleep_logs;
DROP TABLE IF EXISTS sleep_plans;
DROP TABLE IF EXISTS ingredients;
DROP TABLE IF EXISTS reviews;
DROP TABLE IF EXISTS products;

-- ========== OPRET TABELLER (forælder før barn) ==========

CREATE TABLE products (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    price       DOUBLE PRECISION NOT NULL,
    image_url   VARCHAR(300)
);

CREATE TABLE ingredients (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(100) NOT NULL,
    amount      DOUBLE PRECISION NOT NULL,
    unit        VARCHAR(20) NOT NULL,
    type        VARCHAR(50),
    product_id  INTEGER NOT NULL REFERENCES products(id) ON DELETE CASCADE
);

CREATE TABLE reviews (
    id          SERIAL PRIMARY KEY,
    rating      INTEGER NOT NULL CHECK (rating >= 1 AND rating <= 5),
    name        VARCHAR(100) NOT NULL,
    text        VARCHAR(500) NOT NULL,
    date        DATE NOT NULL DEFAULT CURRENT_DATE
);

-- ========== HYDRERING ==========

CREATE TABLE hydration_plans (
    id          SERIAL PRIMARY KEY,
    user_id     UUID REFERENCES auth.users(id) ON DELETE CASCADE NOT NULL,
    start_date  DATE NOT NULL DEFAULT CURRENT_DATE,
    is_active   BOOLEAN NOT NULL DEFAULT true,
    created_at  TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE hydration_logs (
    id          SERIAL PRIMARY KEY,
    plan_id     INTEGER REFERENCES hydration_plans(id) ON DELETE CASCADE NOT NULL,
    user_id     UUID REFERENCES auth.users(id) ON DELETE CASCADE NOT NULL,
    day_number  INTEGER NOT NULL CHECK (day_number BETWEEN 1 AND 30),
    target_ml   INTEGER NOT NULL DEFAULT 2500,
    logged_ml   INTEGER NOT NULL DEFAULT 0,
    log_date    DATE NOT NULL,
    created_at  TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(plan_id, day_number)
);

-- ========== LØB ==========

CREATE TABLE running_plans (
    id          SERIAL PRIMARY KEY,
    user_id     UUID REFERENCES auth.users(id) ON DELETE CASCADE NOT NULL,
    start_date  DATE NOT NULL DEFAULT CURRENT_DATE,
    is_active   BOOLEAN NOT NULL DEFAULT true,
    created_at  TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE running_logs (
    id          SERIAL PRIMARY KEY,
    plan_id     INTEGER REFERENCES running_plans(id) ON DELETE CASCADE NOT NULL,
    user_id     UUID REFERENCES auth.users(id) ON DELETE CASCADE NOT NULL,
    day_number  INTEGER NOT NULL CHECK (day_number BETWEEN 1 AND 30),
    target_km   NUMERIC(4,1) NOT NULL,
    logged_km   NUMERIC(5,2) NOT NULL DEFAULT 0,
    log_date    DATE NOT NULL,
    created_at  TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(plan_id, day_number)
);

-- ========== SØVN ==========

CREATE TABLE sleep_plans (
    id          SERIAL PRIMARY KEY,
    user_id     UUID REFERENCES auth.users(id) ON DELETE CASCADE NOT NULL,
    start_date  DATE NOT NULL DEFAULT CURRENT_DATE,
    is_active   BOOLEAN NOT NULL DEFAULT true,
    created_at  TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE sleep_logs (
    id          SERIAL PRIMARY KEY,
    plan_id     INTEGER REFERENCES sleep_plans(id) ON DELETE CASCADE NOT NULL,
    user_id     UUID REFERENCES auth.users(id) ON DELETE CASCADE NOT NULL,
    day_number  INTEGER NOT NULL CHECK (day_number BETWEEN 1 AND 30),
    target_hours NUMERIC(3,1) NOT NULL,
    logged_hours NUMERIC(3,1) NOT NULL DEFAULT 0,
    log_date    DATE NOT NULL,
    created_at  TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(plan_id, day_number)
);

-- ========== CHALLENGES (med category-kolonne) ==========

CREATE TABLE challenges (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    title VARCHAR(100) NOT NULL,
    description TEXT NOT NULL,
    type VARCHAR(50) NOT NULL,
    category VARCHAR(20) NOT NULL DEFAULT 'hydration',
    target_value INT NOT NULL,
    points INT NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE user_challenges (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
    challenge_id UUID NOT NULL REFERENCES challenges(id) ON DELETE CASCADE,
    progress INT DEFAULT 0,
    is_completed BOOLEAN DEFAULT false,
    joined_at TIMESTAMPTZ DEFAULT NOW(),
    completed_at TIMESTAMPTZ,
    UNIQUE(user_id, challenge_id)
);

-- ========== ORDRER ==========

CREATE TABLE orders (
    id SERIAL PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES auth.users(id),
    total_price DOUBLE PRECISION NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'confirmed',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE order_items (
    id SERIAL PRIMARY KEY,
    order_id INTEGER NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id INTEGER NOT NULL,
    product_name VARCHAR(100) NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price DOUBLE PRECISION NOT NULL
);

-- ========== INDSÆT DATA ==========

INSERT INTO products (id, name, description, price, image_url) VALUES
(1, 'LYTE Citrus', '330 ml sukkerfri elektrolytdrik med naturlig citrussmag. 24 dåser.', 449.00, '/images/lyte-citrus.png'),
(2, 'LYTE Drops', '50 ml koncentrat med elektrolytter. Op til 40 doser pr. flaske.', 249.00, '/images/lyte-drops.png');

INSERT INTO ingredients (id, name, amount, unit, type, product_id) VALUES
(1, 'Natrium', 500, 'mg', 'Elektrolyt', 1),
(2, 'Kalium', 200, 'mg', 'Elektrolyt', 1),
(3, 'Magnesium', 60, 'mg', 'Mineral', 1),
(4, 'Vitamin C', 80, 'mg', 'Vitamin', 1),
(5, 'Natrium', 400, 'mg', 'Elektrolyt', 2),
(6, 'Kalium', 180, 'mg', 'Elektrolyt', 2),
(7, 'Magnesium', 50, 'mg', 'Mineral', 2),
(8, 'Præbiotisk fiber', 2, 'g', 'Fiber', 2);

INSERT INTO reviews (id, rating, name, text, date) VALUES
(1, 5, 'Mikkel S.', 'Perfekt til efter træning. Kan virkelig mærke forskel på energiniveauet.', '2026-04-15'),
(2, 4, 'Laura K.', 'God smag og dejligt at det er uden sukker. Kunne godt bruge flere smagsvarianter.', '2026-04-20'),
(3, 5, 'Jonas P.', 'Bruger det hver dag på kontoret. Meget bedre end den tredje kop kaffe.', '2026-04-28');

-- Hydrering-challenges
INSERT INTO challenges (title, description, type, category, target_value, points) VALUES
    ('C1, Vandvane', 'Nå dit daglige mål 7 dage i træk', 'streak', 'hydration', 7, 100),
    ('C2, Hydreringsmester', 'Drik mindst 3000 ml på én dag', 'single', 'hydration', 3000, 50),
    ('C3, Streaker', 'Opbyg en streak på 14 dage i træk', 'streak', 'hydration', 14, 200),
    ('C4, Marathonløber', 'Fuldfør 30-dages planen med mindst 20 dage klaret', 'plan', 'hydration', 20, 250);

-- Løb-challenges
INSERT INTO challenges (title, description, type, category, target_value, points) VALUES
    ('Løbestarter', 'Løb hver dag i 7 dage i træk', 'streak', 'running', 7, 50),
    ('Maratonvilje', 'Løb hver dag i 14 dage i træk', 'streak', 'running', 14, 120),
    ('Sprintmester', 'Løb 3 km på én dag', 'single', 'running', 3, 80),
    ('Udholdenheds-helt', 'Gennemfør 20 af 30 dages løbeplan', 'plan', 'running', 20, 200);

-- Søvn-challenges
INSERT INTO challenges (title, description, type, category, target_value, points) VALUES
    ('Søvnrytme', 'Nå søvnmålet 7 nætter i træk', 'streak', 'sleep', 7, 50),
    ('Søvnmester', 'Nå søvnmålet 14 nætter i træk', 'streak', 'sleep', 14, 120),
    ('Dyb søvn', 'Sov 9 timer på én nat', 'single', 'sleep', 9, 80),
    ('Hvile-helt', 'Gennemfør 20 af 30 dages søvnplan', 'plan', 'sleep', 20, 200);

-- ========== ROW LEVEL SECURITY ==========

-- Hydrering
ALTER TABLE hydration_plans ENABLE ROW LEVEL SECURITY;
ALTER TABLE hydration_logs ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can manage own plans"
    ON hydration_plans FOR ALL
    USING (auth.uid() = user_id);

CREATE POLICY "Users can manage own logs"
    ON hydration_logs FOR ALL
    USING (auth.uid() = user_id);

-- Løb
ALTER TABLE running_plans ENABLE ROW LEVEL SECURITY;
ALTER TABLE running_logs ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can manage own running plans"
    ON running_plans FOR ALL
    USING (auth.uid() = user_id);

CREATE POLICY "Users can manage own running logs"
    ON running_logs FOR ALL
    USING (auth.uid() = user_id);

-- Søvn
ALTER TABLE sleep_plans ENABLE ROW LEVEL SECURITY;
ALTER TABLE sleep_logs ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can manage own sleep plans"
    ON sleep_plans FOR ALL
    USING (auth.uid() = user_id);

CREATE POLICY "Users can manage own sleep logs"
    ON sleep_logs FOR ALL
    USING (auth.uid() = user_id);

-- Challenges
ALTER TABLE challenges ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_challenges ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Alle kan se aktive challenges"
    ON challenges FOR SELECT
    TO authenticated
    USING (is_active = true);

CREATE POLICY "Brugere ser egne challenges"
    ON user_challenges FOR SELECT
    TO authenticated
    USING (user_id = auth.uid());

CREATE POLICY "Brugere kan tilmelde sig challenges"
    ON user_challenges FOR INSERT
    TO authenticated
    WITH CHECK (user_id = auth.uid());

CREATE POLICY "Brugere kan opdatere egne challenges"
    ON user_challenges FOR UPDATE
    TO authenticated
    USING (user_id = auth.uid());

-- Ordrer
ALTER TABLE orders ENABLE ROW LEVEL SECURITY;
ALTER TABLE order_items ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Brugere kan se egne ordrer" ON orders
    FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Brugere kan oprette ordrer" ON orders
    FOR INSERT WITH CHECK (auth.uid() = user_id);

CREATE POLICY "Brugere kan se egne ordre-varer" ON order_items
    FOR SELECT USING (
      EXISTS (SELECT 1 FROM orders WHERE orders.id = order_items.order_id AND orders.user_id = auth.uid())
    );

CREATE POLICY "Brugere kan tilføje ordre-varer" ON order_items
    FOR INSERT WITH CHECK (
      EXISTS (SELECT 1 FROM orders WHERE orders.id = order_items.order_id AND orders.user_id = auth.uid())
    );
