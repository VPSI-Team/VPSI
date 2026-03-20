-- V001: Testovací data pro lokální vývoj
-- Tento soubor obsahuje seed data pro všech 11 tabulek parkovacího systému VPSI.
-- Spouštět pouze na vývojovém / testovacím prostředí.

BEGIN;

-- ============================================================================
-- 1. parking_lot — jedno testovací parkoviště
-- ============================================================================
INSERT INTO parking_lot (id, name, address, capacity_total, timezone)
VALUES (
    'a1b2c3d4-1234-5678-9abc-def012345678',
    'Parkoviště Centrum',
    'Hlavní 123, Praha 1',
    100,
    'Europe/Prague'
);

-- ============================================================================
-- 2. parking_spot — 100 parkovacích míst (80 STANDARD + 10 EV + 10 ZTP)
-- ============================================================================

-- 80x STANDARD míst: A-001 až A-080
INSERT INTO parking_spot (id, parking_lot_id, code, spot_type, is_active)
SELECT
    gen_random_uuid(),
    'a1b2c3d4-1234-5678-9abc-def012345678',
    'A-' || lpad(gs::text, 3, '0'),
    'STANDARD',
    true
FROM generate_series(1, 80) AS gs;

-- 10x EV míst: EV-001 až EV-010
INSERT INTO parking_spot (id, parking_lot_id, code, spot_type, is_active)
SELECT
    gen_random_uuid(),
    'a1b2c3d4-1234-5678-9abc-def012345678',
    'EV-' || lpad(gs::text, 3, '0'),
    'EV',
    true
FROM generate_series(1, 10) AS gs;

-- 10x ZTP míst: ZTP-001 až ZTP-010
INSERT INTO parking_spot (id, parking_lot_id, code, spot_type, is_active)
SELECT
    gen_random_uuid(),
    'a1b2c3d4-1234-5678-9abc-def012345678',
    'ZTP-' || lpad(gs::text, 3, '0'),
    'ZTP',
    true
FROM generate_series(1, 10) AS gs;

-- ============================================================================
-- 3. vehicle — testovací vozidla
-- ============================================================================
INSERT INTO vehicle (id, plate_number, country_code, plate_hash, vehicle_group) VALUES
    ('e1e1e1e1-aaaa-bbbb-cccc-dddddddddd01', '1AB2345', 'CZ', encode(sha256('1AB2345'::bytea), 'hex'), NULL),
    ('e1e1e1e1-aaaa-bbbb-cccc-dddddddddd02', '2CD6789', 'CZ', encode(sha256('2CD6789'::bytea), 'hex'), 'EMPLOYEE');

-- ============================================================================
-- 4. device — 5 zařízení odpovídajících HW simulátoru
-- UUID hodnoty musí odpovídat hw-simulator/src/HwSimulator.App/Models/SimulatorOptions.cs
-- ============================================================================
INSERT INTO device (id, parking_lot_id, name, device_type, protocol, config) VALUES
    ('b5e1a4a2-8c4f-4b1c-8b1a-9b4b4e8f4a11', 'a1b2c3d4-1234-5678-9abc-def012345678',
     'LPR Vjezd', 'LPR', 'REST', '{"lane": "ENTRY_1", "direction": "IN"}'),
    ('b5e1a4a2-8c4f-4b1c-8b1a-9b4b4e8f4a12', 'a1b2c3d4-1234-5678-9abc-def012345678',
     'LPR Výjezd', 'LPR', 'REST', '{"lane": "EXIT_1", "direction": "OUT"}'),
    ('c6f2b5b3-9d5f-4c2d-9c2b-0c5c5f9f5b22', 'a1b2c3d4-1234-5678-9abc-def012345678',
     'Závora Vjezd', 'BARRIER', 'REST', '{"lane": "ENTRY_1"}'),
    ('c6f2b5b3-9d5f-4c2d-9c2b-0c5c5f9f5b23', 'a1b2c3d4-1234-5678-9abc-def012345678',
     'Závora Výjezd', 'BARRIER', 'REST', '{"lane": "EXIT_1"}'),
    ('d7f3c6c4-ae6f-4d3e-ad3c-1d6d6faf6c33', 'a1b2c3d4-1234-5678-9abc-def012345678',
     'Senzor obsazenosti', 'SENSOR', 'MQTT', '{"zone": "MAIN"}');

-- ============================================================================
-- 5. tariff + tariff_rule — cenové tarify
-- ============================================================================

-- Základní tarif
INSERT INTO tariff (id, parking_lot_id, name, description, valid_from) VALUES
    ('f1f1f1f1-1111-2222-3333-444444444401', 'a1b2c3d4-1234-5678-9abc-def012345678',
     'Základní tarif', 'Standardní hodinová sazba', '2026-01-01T00:00:00Z');

INSERT INTO tariff_rule (id, tariff_id, rule_type, amount, parameters, priority) VALUES
    (gen_random_uuid(), 'f1f1f1f1-1111-2222-3333-444444444401', 'FREE_MINUTES', 0, '{"minutes": 15}', 0),
    (gen_random_uuid(), 'f1f1f1f1-1111-2222-3333-444444444401', 'PER_HOUR', 40.00, '{}', 1),
    (gen_random_uuid(), 'f1f1f1f1-1111-2222-3333-444444444401', 'DAILY_CAP', 200.00, '{}', 2);

-- Noční tarif
INSERT INTO tariff (id, parking_lot_id, name, description, valid_from) VALUES
    ('f1f1f1f1-1111-2222-3333-444444444402', 'a1b2c3d4-1234-5678-9abc-def012345678',
     'Noční tarif', 'Zvýhodněný noční tarif 22:00-06:00', '2026-01-01T00:00:00Z');

INSERT INTO tariff_rule (id, tariff_id, rule_type, amount, parameters, priority) VALUES
    (gen_random_uuid(), 'f1f1f1f1-1111-2222-3333-444444444402', 'NIGHT_RATE', 20.00, '{"from_hour": 22, "to_hour": 6}', 0),
    (gen_random_uuid(), 'f1f1f1f1-1111-2222-3333-444444444402', 'DAILY_CAP', 100.00, '{}', 1);

-- ============================================================================
-- 6. app_user — testovací uživatelé systému
-- ============================================================================
INSERT INTO app_user (id, external_subject, display_name, email, role) VALUES
    ('d1d1d1d1-aaaa-bbbb-cccc-dddddddddd01', 'auth0|admin001', 'Jan Správce', 'admin@vpsi.local', 'ADMIN'),
    ('d1d1d1d1-aaaa-bbbb-cccc-dddddddddd02', 'auth0|tech001', 'Petr Technik', 'technik@vpsi.local', 'TECHNICIAN');

-- ============================================================================
-- 7. parking_session — ukázkové parkovací relace
-- ============================================================================

-- ACTIVE relace — vozidlo 1AB2345 aktuálně zaparkované (vjezd 15.3.2026 08:30)
INSERT INTO parking_session (id, parking_lot_id, vehicle_id, tariff_id, entry_device_id, entry_at, status) VALUES
    ('aaa11111-1111-1111-1111-111111111101',
     'a1b2c3d4-1234-5678-9abc-def012345678',
     'e1e1e1e1-aaaa-bbbb-cccc-dddddddddd01',
     'f1f1f1f1-1111-2222-3333-444444444401',
     'b5e1a4a2-8c4f-4b1c-8b1a-9b4b4e8f4a11',
     '2026-03-15T08:30:00Z',
     'ACTIVE');

-- CLOSED relace — vozidlo 2CD6789, zaparkováno 14.3. 10:00–12:30 (2,5h = 100 Kč)
INSERT INTO parking_session (id, parking_lot_id, vehicle_id, tariff_id, entry_device_id, exit_device_id, entry_at, exit_at, status, total_amount, paid_at) VALUES
    ('aaa11111-1111-1111-1111-111111111102',
     'a1b2c3d4-1234-5678-9abc-def012345678',
     'e1e1e1e1-aaaa-bbbb-cccc-dddddddddd02',
     'f1f1f1f1-1111-2222-3333-444444444401',
     'b5e1a4a2-8c4f-4b1c-8b1a-9b4b4e8f4a11',
     'b5e1a4a2-8c4f-4b1c-8b1a-9b4b4e8f4a12',
     '2026-03-14T10:00:00Z',
     '2026-03-14T12:30:00Z',
     'CLOSED',
     100.00,
     '2026-03-14T12:28:00Z');

-- ============================================================================
-- 8. payment_intent — platba za uzavřenou relaci
-- ============================================================================
INSERT INTO payment_intent (id, parking_session_id, amount, currency, method, provider_ref, status) VALUES
    (gen_random_uuid(),
     'aaa11111-1111-1111-1111-111111111102',
     100.00,
     'CZK',
     'CARD',
     'psp-test-20260314-001',
     'CAPTURED');

-- ============================================================================
-- 9. device_event — ukázkové události zařízení
-- Časové značky v rozsahu 2026-01 až 2026-12 (existující partitions)
-- ============================================================================

-- Události pro ACTIVE relaci (vjezd 1AB2345)
INSERT INTO device_event (id, device_id, event_type, occurred_at, idempotency_key, plate_number, payload) VALUES
    (gen_random_uuid(),
     'b5e1a4a2-8c4f-4b1c-8b1a-9b4b4e8f4a11',
     'LPR_READ',
     '2026-03-15T08:30:00Z',
     'lpr-entry-20260315-083000-1AB2345',
     '1AB2345',
     '{"plate": "1AB2345", "confidence": 0.97, "direction": "IN"}'),
    (gen_random_uuid(),
     'c6f2b5b3-9d5f-4c2d-9c2b-0c5c5f9f5b22',
     'BARRIER_OPEN',
     '2026-03-15T08:30:02Z',
     'barrier-entry-20260315-083002',
     NULL,
     '{"lane": "ENTRY_1", "trigger": "LPR"}');

-- Události pro CLOSED relaci (vjezd + výjezd 2CD6789)
INSERT INTO device_event (id, device_id, event_type, occurred_at, idempotency_key, plate_number, payload) VALUES
    (gen_random_uuid(),
     'b5e1a4a2-8c4f-4b1c-8b1a-9b4b4e8f4a11',
     'LPR_READ',
     '2026-03-14T10:00:00Z',
     'lpr-entry-20260314-100000-2CD6789',
     '2CD6789',
     '{"plate": "2CD6789", "confidence": 0.95, "direction": "IN"}'),
    (gen_random_uuid(),
     'c6f2b5b3-9d5f-4c2d-9c2b-0c5c5f9f5b22',
     'BARRIER_OPEN',
     '2026-03-14T10:00:02Z',
     'barrier-entry-20260314-100002',
     NULL,
     '{"lane": "ENTRY_1", "trigger": "LPR"}'),
    (gen_random_uuid(),
     'b5e1a4a2-8c4f-4b1c-8b1a-9b4b4e8f4a12',
     'LPR_READ',
     '2026-03-14T12:30:00Z',
     'lpr-exit-20260314-123000-2CD6789',
     '2CD6789',
     '{"plate": "2CD6789", "confidence": 0.96, "direction": "OUT"}'),
    (gen_random_uuid(),
     'c6f2b5b3-9d5f-4c2d-9c2b-0c5c5f9f5b23',
     'BARRIER_OPEN',
     '2026-03-14T12:30:02Z',
     'barrier-exit-20260314-123002',
     NULL,
     '{"lane": "EXIT_1", "trigger": "LPR"}');

COMMIT;
