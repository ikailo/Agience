import requests
from langchain.tools import tool
from dotenv import load_dotenv
import os
load_dotenv()

class CoinGeckoAPI:
    """
    A class to interact with the CoinGecko API to fetch cryptocurrency data.
    """
    BASE_URL = "https://api.coingecko.com/api/v3"
    
    def __init__(self, api_key: str):
        """
        Initializes the CoinGeckoAPI instance with the provided API key.
        """
        self.api_key = api_key
        self.headers = {"Authorization": f"Bearer {self.api_key}"}
    
    def _get(self, endpoint: str, params: dict = {}):
        """
        Sends a GET request to the CoinGecko API.
        
        :param endpoint: The API endpoint to query.
        :param params: Additional parameters for the request.
        :return: JSON response or error message.
        """
        url = f"{self.BASE_URL}{endpoint}"
        params["x_cg_pro_api_key"] = self.api_key  
        response = requests.get(url, headers=self.headers, params=params)
        
        if response.status_code == 200:
            return response.json()
        else:
            return {"error": response.json(), "status_code": response.status_code}

    @tool
    def ping(self):
        """
        Checks API server status.
        """
        return self._get("/ping")

    @tool
    def get_price(self, ids: str, vs_currencies: str):
        """
        Fetches the price of a cryptocurrency.
        
        :param ids: Cryptocurrency ID (e.g., "bitcoin").
        :param vs_currencies: Currency to compare against (e.g., "usd").
        :return: Price data.
        """
        return self._get("/simple/price", {"ids": ids, "vs_currencies": vs_currencies})

    @tool
    def get_supported_currencies(self):
        """
        Retrieves a list of supported fiat and cryptocurrency conversion options.
        """
        return self._get("/simple/supported_vs_currencies")

    @tool
    def get_coins_list(self):
        """
        Fetches a list of all available cryptocurrencies on CoinGecko.
        """
        return self._get("/coins/list")
    
    @tool
    def get_coin_data(self, coin_id: str):
        """
        Retrieves detailed data for a specific cryptocurrency.
        
        :param coin_id: Cryptocurrency ID (e.g., "bitcoin").
        :return: Detailed coin data.
        """
        return self._get(f"/coins/{coin_id}")
    
    @tool
    def get_coin_market_data(self, vs_currency: str, order: str = "market_cap_desc", per_page: int = 100, page: int = 1):
        """
        Fetches market data for cryptocurrencies.
        
        :param vs_currency: Currency for comparison (e.g., "usd").
        :param order: Sorting order (default: "market_cap_desc").
        :param per_page: Number of items per page (default: 100).
        :param page: Page number (default: 1).
        :return: Market data for cryptocurrencies.
        """
        return self._get("/coins/markets", {"vs_currency": vs_currency, "order": order, "per_page": per_page, "page": page})
    
    @tool
    def get_coin_history(self, coin_id: str, date: str):
        """
        Retrieves historical market data for a given date.
        
        :param coin_id: Cryptocurrency ID.
        :param date: Date in format "dd-mm-yyyy".
        :return: Historical coin data.
        """
        return self._get(f"/coins/{coin_id}/history", {"date": date})
    
    @tool
    def get_market_chart(self, coin_id: str, vs_currency: str, days: int):
        """
        Retrieves market chart data for a given number of days.
        
        :param coin_id: Cryptocurrency ID.
        :param vs_currency: Currency for comparison.
        :param days: Number of past days to fetch data for.
        :return: Market chart data.
        """
        return self._get(f"/coins/{coin_id}/market_chart", {"vs_currency": vs_currency, "days": days})
    
    @tool
    def get_market_chart_range(self, coin_id: str, vs_currency: str, from_timestamp: int, to_timestamp: int):
        """
        Retrieves market chart data for a specified time range.
        
        :param coin_id: Cryptocurrency ID.
        :param vs_currency: Currency for comparison.
        :param from_timestamp: Start timestamp.
        :param to_timestamp: End timestamp.
        :return: Market chart data for the specified time range.
        """
        return self._get(f"/coins/{coin_id}/market_chart/range", {"vs_currency": vs_currency, "from": from_timestamp, "to": to_timestamp})
    
    @tool
    def get_coin_tickers(self, coin_id: str):
        """
        Fetches tickers for a specific cryptocurrency.
        
        :param coin_id: Cryptocurrency ID.
        :return: List of coin tickers.
        """
        return self._get(f"/coins/{coin_id}/tickers")
    
    @tool
    def get_coin_categories(self):
        """
        Retrieves a list of available cryptocurrency categories.
        """
        return self._get("/coins/categories")

# Load API Key from .env
api_key = os.getenv("COINGECKO_API_KEY")

# Ensure API key is available
if not api_key:
    raise ValueError("COINGECKO_API_KEY is missing! Please check your .env file.")

# Instantiate API with the key
cg = CoinGeckoAPI(api_key)

# List of tools properly initialized
tools = [
    cg.ping,
    cg.get_price,
    cg.get_supported_currencies,
    cg.get_coins_list,
    cg.get_coin_data,
    cg.get_coin_market_data,
    cg.get_coin_history,
    cg.get_market_chart,
    cg.get_market_chart_range,
    cg.get_coin_tickers,
    cg.get_coin_categories,
]

# Example usage
if __name__ == "__main__":
    print(cg.ping())  # Test API connectivity
