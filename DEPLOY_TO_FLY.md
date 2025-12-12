# Deploying the Database to Fly.io

Since **CrossSetaDeduplicator** is a desktop application, it cannot be hosted on Fly.io directly. However, to enable the **Cross-SETA** functionality (where multiple users connect to the same data), we must deploy the **SQL Server Database** to the cloud.

This guide explains how to deploy the database to Fly.io using the Docker configuration we created.

## Prerequisites
1.  **Fly.io Account**: Sign up at [fly.io](https://fly.io).
2.  **Fly CLI**: Install the command line tool.
    *   **Mac**: `brew install flyctl`
    *   **Windows**: `iwr https://fly.io/install.ps1 -useb | iex`

## Step-by-Step Deployment

### 1. Initialize the App
Open your terminal in the project root and run:

```bash
fly launch --dockerfile docker/mssql/Dockerfile --no-deploy
```

*   **App Name**: Give it a unique name (e.g., `cross-seta-db`).
*   **Region**: Choose one close to you (e.g., `jnb` for Johannesburg).
*   **Database**: Select "No" (we are deploying *a* database container, not using Fly Postgres).
*   **Redis**: Select "No".

### 2. Configure Storage (Critical)
SQL Server needs a persistent volume to store data.

```bash
fly volumes create mssql_data --size 10 --region jnb
```
*(Replace `jnb` with your chosen region)*

Edit the `fly.toml` file generated in your root directory. Add the mount configuration:

```toml
[mounts]
  source = "mssql_data"
  destination = "/var/opt/mssql"
```

### 3. Set Secrets
Set the `SA_PASSWORD` (System Administrator Password). Make it strong!

```bash
fly secrets set SA_PASSWORD="YourStrongPassword123!" ACCEPT_EULA=Y
```

### 4. Deploy
Deploy the container to Fly.io.

```bash
fly deploy
```

### 5. Get Connection Details
Once deployed, your database will be accessible.

*   **Hostname**: `cross-seta-db.fly.dev` (or your app name)
*   **Port**: 1433 (Standard SQL Port)

**Note:** By default, Fly.io apps are not exposed on port 1433 publicly unless you configure a public IP.
*   **Easy Way**: Use `fly proxy 1433 -a cross-seta-db` to forward the port to your local machine (`localhost:1433`).
*   **Production Way**: Assign a dedicated IPv4 address (`fly ips allocate-v4`) and ensure the `fly.toml` maps port 1433.

## Updating the App
Once the DB is running, update the `DatabaseHelper.vb` or your environment variable in the Desktop App:

```vb
' Connection String format
"Server=cross-seta-db.fly.dev,1433;Database=CrossSetaDB;User Id=sa;Password=YourStrongPassword123!;"
```

Your Cross-SETA solution is now cloud-connected!

## 🔴 Live Deployment Details (Current Session)

We have successfully deployed the database instance for this session:

*   **App Name**: `cross-seta-db-17655`
*   **Region**: `jnb` (Johannesburg)
*   **Public IP**: `213.188.209.45`
*   **Connection String**:
    ```text
    Server=213.188.209.45,1433;Database=CrossSetaDB;User Id=sa;Password=StrongPassw0rd!123;TrustServerCertificate=True;
    ```
*   **Setup Instruction**:
    Set the environment variable `CROSS_SETA_DB_CONNECTION` on your machine to the connection string above. The application will automatically pick it up.

---

## 🌐 Deploying the Web Application
The Web Portal allows users to register from anywhere. It connects to the same cloud database.

### 1. Navigate to the Web Project
```bash
cd CrossSetaDeduplicator/src/CrossSetaWeb
```

### 2. Launch the App
Initialize the Fly.io application.
```bash
fly launch --no-deploy
```
*   **App Name**: e.g., `cross-seta-web`
*   **Region**: Same as your database (e.g., `jnb`).

### 3. Connect to Database
Securely link the web app to your running database using the internal private network.

```bash
fly secrets set CROSS_SETA_DB_CONNECTION="Server=cross-seta-db-17655.internal;Database=CrossSetaDB;User Id=sa;Password=YourStrongPassword123!;Encrypt=False;"
```
*Note: We use the internal hostname `cross-seta-db-17655.internal` for secure, low-latency communication.*

### 4. Deploy
```bash
fly deploy
```

Your web portal will be live at `https://cross-seta-web.fly.dev/`.
