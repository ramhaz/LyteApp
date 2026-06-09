-- ========== LØBE-TABELLER ==========

CREATE TABLE running_plans (
  id            SERIAL PRIMARY KEY,
  user_id       UUID NOT NULL REFERENCES auth.users(id),
  start_date    DATE NOT NULL,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE running_logs (
  id            SERIAL PRIMARY KEY,
  plan_id       INT NOT NULL REFERENCES running_plans(id) ON DELETE CASCADE,
  user_id       UUID NOT NULL REFERENCES auth.users(id),
  day_number    INT NOT NULL,
  target_km     NUMERIC(4,1) NOT NULL,
  logged_km     NUMERIC(5,2) NOT NULL DEFAULT 0,
  log_date      DATE NOT NULL,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ========== SØVN-TABELLER ==========

CREATE TABLE sleep_plans (
  id            SERIAL PRIMARY KEY,
  user_id       UUID NOT NULL REFERENCES auth.users(id),
  start_date    DATE NOT NULL,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE sleep_logs (
  id            SERIAL PRIMARY KEY,
  plan_id       INT NOT NULL REFERENCES sleep_plans(id) ON DELETE CASCADE,
  user_id       UUID NOT NULL REFERENCES auth.users(id),
  day_number    INT NOT NULL,
  target_hours  NUMERIC(3,1) NOT NULL,
  logged_hours  NUMERIC(3,1) NOT NULL DEFAULT 0,
  log_date      DATE NOT NULL,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ========== TILFØJ CATEGORY TIL CHALLENGES ==========

ALTER TABLE challenges
  ADD COLUMN category VARCHAR(20) NOT NULL DEFAULT 'hydration';

UPDATE challenges SET category = 'hydration';

-- ========== INDSÆT LØB-CHALLENGES ==========

INSERT INTO challenges (id, title, description, type, category, target_value, points, is_active) VALUES
  (gen_random_uuid(), 'Løbestarter', 'Løb hver dag i 7 dage i træk', 'streak', 'running', 7, 50, true),
  (gen_random_uuid(), 'Maratonvilje', 'Løb hver dag i 14 dage i træk', 'streak', 'running', 14, 120, true),
  (gen_random_uuid(), 'Sprintmester', 'Løb 3 km på én dag', 'single', 'running', 3, 80, true),
  (gen_random_uuid(), 'Udholdenheds-helt', 'Gennemfør 20 af 30 dages løbeplan', 'plan', 'running', 20, 200, true);

-- ========== INDSÆT SØVN-CHALLENGES ==========

INSERT INTO challenges (id, title, description, type, category, target_value, points, is_active) VALUES
  (gen_random_uuid(), 'Søvnrytme', 'Nå søvnmålet 7 nætter i træk', 'streak', 'sleep', 7, 50, true),
  (gen_random_uuid(), 'Søvnmester', 'Nå søvnmålet 14 nætter i træk', 'streak', 'sleep', 14, 120, true),
  (gen_random_uuid(), 'Dyb søvn', 'Sov 9 timer på én nat', 'single', 'sleep', 9, 80, true),
  (gen_random_uuid(), 'Hvile-helt', 'Gennemfør 20 af 30 dages søvnplan', 'plan', 'sleep', 20, 200, true);

-- ========== ROW LEVEL SECURITY ==========

ALTER TABLE running_plans ENABLE ROW LEVEL SECURITY;
CREATE POLICY "Users can manage own running plans"
  ON running_plans FOR ALL USING (auth.uid() = user_id);

ALTER TABLE running_logs ENABLE ROW LEVEL SECURITY;
CREATE POLICY "Users can manage own running logs"
  ON running_logs FOR ALL USING (auth.uid() = user_id);

ALTER TABLE sleep_plans ENABLE ROW LEVEL SECURITY;
CREATE POLICY "Users can manage own sleep plans"
  ON sleep_plans FOR ALL USING (auth.uid() = user_id);

ALTER TABLE sleep_logs ENABLE ROW LEVEL SECURITY;
CREATE POLICY "Users can manage own sleep logs"
  ON sleep_logs FOR ALL USING (auth.uid() = user_id);
