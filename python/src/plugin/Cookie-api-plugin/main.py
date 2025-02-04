from fastapi import FastAPI, HTTPException  # Import HTTPException
from plugins.cookie_api.cookie_api import fetch_from_cookie_api

app = FastAPI(title="Cookie API Plugin", description="A plugin to interact with Cookie API", version="1.0")

# Root endpoint
@app.get("/")
async def read_root():
    return {"message": "Welcome to the Cookie API Plugin!"}

# Endpoint to fetch Twitter cookie based on the username
@app.get("/cookie/twitter/{username}")
async def get_twitter_cookie(username: str, interval: str = "_7Days"):
    try:
        # Include the full URL, using BASE_URL
        url = f"v2/agents/twitterUsername/{username}?interval={interval}"
        data = fetch_from_cookie_api(url)
        return {"data": data}
    except HTTPException as e:
        raise HTTPException(status_code=e.status_code, detail=str(e))

# Endpoint to fetch contract cookie based on contract address
@app.get("/cookie/contract/{contract_address}")
async def get_contract_cookie(contract_address: str, interval: str = "_7Days"):
    try:
        # Include the full URL, using BASE_URL
        url = f"v2/agents/contractAddress/{contract_address}?interval={interval}"
        data = fetch_from_cookie_api(url)
        return {"data": data}
    except HTTPException as e:
        raise HTTPException(status_code=e.status_code, detail=str(e))

# Endpoint to fetch a paginated list of agents
@app.get("/cookie/agents")
async def get_agents(interval: str = "_7Days", page: int = 1, page_size: int = 10):
    try:
        params = {"interval": interval, "page": page, "pageSize": page_size}
        # Use BASE_URL and the endpoint
        url = f"v2/agents/agentsPaged"
        data = fetch_from_cookie_api(url, params)
        return {"data": data}
    except HTTPException as e:
        raise HTTPException(status_code=e.status_code, detail=str(e))

# Endpoint to search cookie data based on a query and date range
@app.get("/cookie/search")
async def search_cookie_token(query: str, from_date: str, to_date: str):
    try:
        params = {"from": from_date, "to": to_date}
        # URL encode spaces in the query
        search_query = query.replace(' ', '%20')
        # Include BASE_URL in the request
        url = f"v1/hackathon/search/{search_query}"
        data = fetch_from_cookie_api(url, params)
        return {"data": data}
    except HTTPException as e:
        raise HTTPException(status_code=e.status_code, detail=str(e))

# Run the FastAPI server if executed directly
if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)