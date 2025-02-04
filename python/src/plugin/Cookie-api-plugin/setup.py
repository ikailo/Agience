from setuptools import setup, find_packages

setup(
    name="cookie_api_plugin",
    version="0.1.0",
    packages=find_packages(),
    install_requires=[
        "requests",
        "fastapi",
        "python-dotenv",
        "uvicorn"
    ],
    include_package_data=True,
    description="A plugin for fetching cookie API data",
    author="Aakash Jaiswal",
    author_email="jaiswalraj03014@gmail.com",
    url="https://github.com/jaiswalraj03014/cookie_api_plugin", 
)