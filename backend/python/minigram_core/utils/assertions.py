import uuid


def throw_if_null_or_empty(value: uuid.UUID | None, param_name: str | None = None) -> None:
    name = param_name if param_name else "Guid"
    message = f"{name} cannot be null or empty"

    if value is None or value == uuid.UUID(int=0):
        raise ValueError(message)
