import requests

class APIConnectorService:
    def __init__(self, api_key, base_url):
        self.api_key = api_key
        self.base_url = base_url

    def get_response_for(self, endpoint, endpoint_params=''):
        headers = {'x-api-key': self.api_key}
        response = requests.get(
            f'{self.base_url}/{endpoint}',
            headers=headers,
            params=endpoint_params
        )
        return response
    
    def get_json_response(self, endpoint):
        response = self.get_response_for(endpoint)
        if response.status_code == 200:
            json = response.json()
            return json
        raise Exception(f'Error raised for {self.base_url}/{endpoint}', response)