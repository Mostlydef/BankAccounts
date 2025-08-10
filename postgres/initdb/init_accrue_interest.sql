CREATE OR REPLACE PROCEDURE accrue_interest(p_account_id UUID)
LANGUAGE plpgsql
AS $$
DECLARE
v_balance       NUMERIC;
  v_interest_rate NUMERIC;
  v_interest      NUMERIC;
  v_currency      TEXT;
BEGIN
SELECT "Balance", "InterestRate", "Currency"
INTO v_balance, v_interest_rate, v_currency
FROM "Accounts"
WHERE "Id" = p_account_id
  AND "InterestRate" IS NOT NULL;

IF NOT FOUND THEN
    RAISE EXCEPTION 'Account % not found or has no interest rate', p_account_id;
END IF;

  v_interest := v_balance * v_interest_rate;

UPDATE "Accounts"
SET "Balance" = "Balance" + v_interest
WHERE "Id" = p_account_id;

INSERT INTO "Transactions"
("Id", "AccountId", "Amount", "Currency", "Type", "Description", "Timestamp")
VALUES
    (gen_random_uuid(), p_account_id, v_interest, v_currency, 1, 'Accrued interest', NOW());
END;
$$;
