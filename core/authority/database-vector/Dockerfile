# Use the official Milvus standalone image as the base
FROM milvusdb/milvus:v2.4.0

# Set environment variables for Milvus
# These can be overridden during container startup if needed
ENV MILVUS_LOG_LEVEL=info
ENV MILVUS_CACHE_SIZE=4GB
ENV MILVUS_DATA_PATH=/var/lib/milvus

# Expose Milvus default ports
EXPOSE 19530 9091

# Default CMD to start Milvus
CMD ["/milvus/bin/milvus", "run", "standalone"]
