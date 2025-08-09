CREATE OR REPLACE PROCEDURE accrue_interest(p_account_id UUID)
LANGUAGE plpgsql
AS $$
DECLARE
  v_balance       NUMERIC;
  v_interest_rate NUMERIC;
  v_interest      NUMERIC;
  v_currency      TEXT;
BEGIN
  -- Извлекаем баланс, ставку и валюту
  SELECT balance, interest_rate, currency
    INTO v_balance, v_interest_rate, v_currency
  FROM accounts
  WHERE id = p_account_id
    AND interest_rate IS NOT NULL;

  IF NOT FOUND THEN
    RAISE EXCEPTION 'Account % not found or has no interest rate', p_account_id;
  END IF;

  -- Вычисляем сумму процентов
  v_interest := v_balance * v_interest_rate;

  -- Обновляем баланс
  UPDATE accounts
    SET balance = balance + v_interest
    WHERE id = p_account_id;

  -- Записываем транзакцию с использованием валюты счёта
  INSERT INTO transactions
    (id, account_id, amount, currency, type, description, timestamp)
  VALUES
    (gen_random_uuid(), p_account_id, v_interest, v_currency, 'Credit', 'Accrued interest', NOW());
END;
$$;
