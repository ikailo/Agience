The provided README.md content looks mostly correct for your code, with a few adjustments needed to match the details in your code. Here’s the modified version, tailored specifically for the given code:

# Cookie API Plugin

A **FastAPI** based plugin to interact with the **Cookie API**. This plugin provides multiple endpoints to fetch data from the Cookie API, such as:

- Fetching Twitter cookies based on the username.
- Fetching contract cookies using a contract address.
- Fetching a paginated list of agents.
- Searching cookie data based on a query and date range.

## Requirements

- Python 3.7 or higher
- FastAPI
- Uvicorn
- Requests
- Python-dotenv

## Installation

### Clone the repository:
```bash
git clone <repository_url>
cd <repository_name>

Install dependencies:

Make sure you have Python 3.7 or higher installed. Then install the required dependencies:

pip install -r requirements.txt

Environment Configuration:

Create a .env file in the root directory of the project and add your Cookie API key:

COOKIE_API_KEY=your_cookie_api_key_here

Running the API

To run the FastAPI server, execute the following command:

uvicorn main:app --reload

This will start the server on http://localhost:8000, and you can use the interactive documentation provided by FastAPI at:

http://localhost:8000/docs

Available Endpoints

1. Root Endpoint
	•	URL: /
	•	Method: GET
	•	Description: Returns a welcome message to confirm the server is running.

Example Response:

{
    "message": "Welcome to the Cookie API Plugin!"
}

2. Get Twitter Cookie by Username
	•	URL: /cookie/twitter/{username}
	•	Method: GET
	•	Parameters:
	•	username: The Twitter username.
	•	interval (Optional): The time range for the data, default is _7Days.
	•	Description: Fetches Twitter cookies based on the username provided.

Example:

GET /cookie/twitter/johndoe?interval=_30Days

Example Response:

{
    "data": {
        // Twitter cookie data
    }
}

3. Get Contract Cookie by Contract Address
	•	URL: /cookie/contract/{contract_address}
	•	Method: GET
	•	Parameters:
	•	contract_address: The contract address of the token.
	•	interval (Optional): The time range for the data, default is _7Days.
	•	Description: Fetches contract cookies based on the contract address provided.

Example:

GET /cookie/contract/0x1234abcd?interval=_7Days

Example Response:

{
    "data": {
        // Contract cookie data
    }
}

4. Get Paginated List of Agents
	•	URL: /cookie/agents
	•	Method: GET
	•	Parameters:
	•	interval (Optional): The time range for the data, default is _7Days.
	•	page (Optional): The page number for pagination, default is 1.
	•	page_size (Optional): The number of agents per page, default is 10.
	•	Description: Fetches a paginated list of agents.

Example:

GET /cookie/agents?page=2&page_size=5&interval=_30Days

Example Response:

{
    "data": {
        // Paginated list of agents
    }
}

5. Search Cookie Token
	•	URL: /cookie/search
	•	Method: GET
	•	Parameters:
	•	query: The search query (e.g., a specific token or contract).
	•	from_date: The start date for the search (format: YYYY-MM-DD).
	•	to_date: The end date for the search (format: YYYY-MM-DD).
	•	Description: Searches cookie data based on the query and the provided date range.

Example:

GET /cookie/search?query=Bitcoin&from_date=2023-01-01&to_date=2023-12-31

Example Response:

{
    "data": {
        // Search results
    }
}

Contributing

Feel free to fork this repository, make improvements, and submit pull requests. Contributions are welcome!

License

This project is licensed under the MIT License - see the LICENSE file for details.

