CREATE TABLE links (
  id          uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
  slug        text        UNIQUE NOT NULL,
  destination text        NOT NULL,
  created_at  timestamptz DEFAULT now(),
  expires_at  timestamptz,
  click_count int         NOT NULL DEFAULT 0
);
