FROM liquibase/liquibase:latest-alpine

# Устанавливаем PostgreSQL драйвер
RUN lpm add postgresql --global

# Создаем директории
WORKDIR /liquibase

# Копируем конфигурацию
COPY changelog/liquibase.properties /liquibase/changelog/

# Скрипт ожидания PostgreSQL
# COPY wait-for-postgres.sh /liquibase/
# RUN chmod +x /liquibase/wait-for-postgres.sh

ENTRYPOINT ["/liquibase/liquibase"]