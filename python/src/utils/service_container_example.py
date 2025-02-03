from typing import List
from abc import ABC, abstractmethod
from pydantic import BaseModel

from utils.service_container import ServiceCollection, ServiceProvider


class EmailConfig(BaseModel):
    smtp_server: str
    smtp_port: int
    username: str
    password: str


class IEmailService(ABC):
    @abstractmethod
    def send_email(self, to: str, subject: str, body: str) -> None:
        pass


class IUserService(ABC):
    @abstractmethod
    def get_users(self) -> List[str]:
        pass


class EmailService(IEmailService):
    def __init__(self, config: EmailConfig):
        self.config = config
        # Initialize SMTP client with config

    def send_email(self, to: str, subject: str, body: str) -> None:
        print(f"Sending email using {self.config.smtp_server}")
        # Implement email sending logic


class UserService(IUserService):
    def __init__(self, email_service: IEmailService):
        self.email_service = email_service

    def get_users(self) -> List[str]:
        users = ["user1@example.com", "user2@example.com"]
        return users


def configure_services() -> ServiceCollection:
    services = ServiceCollection()

    # Register email config factory
    def email_config_factory(provider: ServiceProvider) -> EmailConfig:
        return EmailConfig(
            smtp_server="smtp.example.com",
            smtp_port=587,
            username="user",
            password="pass"
        )

    services.add_singleton_factory(EmailConfig, email_config_factory)
    services.add_singleton(IEmailService, EmailService)
    services.add_scoped(IUserService, UserService)

    return services


def main():
    # Configure services
    services = configure_services()
    container = services.build()

    # Use services in root scope
    with container.create_scope() as scope:
        user_service = scope.get_service(IUserService)
        users = user_service.get_users()
        print(f"Found users: {users}")

        # Email service is singleton, will be reused
        email_service = scope.get_service(IEmailService)
        email_service.send_email("test@example.com", "Hello", "World")

    # Create another scope
    with container.create_scope() as scope:
        # This will create a new UserService instance
        user_service = scope.get_service(IUserService)
        # But will reuse the same EmailService instance

    new_service = container.get_service(IUserService)
    new_users = new_service.get_users()
    print(f"Found users: {new_users}")


if __name__ == "__main__":
    main()
