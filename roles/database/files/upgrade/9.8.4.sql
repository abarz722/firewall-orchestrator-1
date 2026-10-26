ALTER TABLE request.state_matrix_phase
    ADD COLUMN IF NOT EXISTS promotion_mode varchar NOT NULL DEFAULT 'Both';
