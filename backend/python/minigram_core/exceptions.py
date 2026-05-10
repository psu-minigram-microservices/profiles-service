from typing import Any


class EntityNotFoundException(Exception):
    def __init__(
        self,
        entity: type | str | None = None,
        entity_id: Any | None = None,
        field_name: str | None = None,
    ) -> None:
        self.entity = entity
        self.entity_id = entity_id
        self.field_name = field_name

        if entity is None:
            message = "The entity was not found."
        elif isinstance(entity, str) and field_name is not None:
            message = f"{entity} with {field_name} '{entity_id}' was not found."
        else:
            entity_name = entity.__name__ if isinstance(entity, type) else str(entity)
            if entity_id is not None:
                message = f"{entity_name} with Id '{entity_id}' was not found."
            else:
                message = f"{entity_name} was not found."

        super().__init__(message)
