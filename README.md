# rs-legacy-order-service

# Prerequisites
- dotnet version: 10.0.102

## Added features
- As cli application, we should use the single database target. I replaced the database source in the built folder with the datasource in the local machine application data directory
- Enable WAL mode to increase the performance, it's safe to use with non-network storage volume
- Make sure if it's hosted on the EC2 machine, the data folder is mounted to external volume

## CLI use cases
- This cli/console application can be hosted on the EC2 machine with EBS to process orders
- This console application can be used to interact and input the order manually