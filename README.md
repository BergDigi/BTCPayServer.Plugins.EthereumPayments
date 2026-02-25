# BTCPayServer.Plugins.EthereumPayments
A ETH, USDT and dEURO Plugin für BTCPayServer


# BTCPayServer Plugin: Ethereum Payments (MVP)

Accept **ETH**, **USDT**, and **dEURO** payments on Ethereum Mainnet in your BTCPayServer store.

## Features

✅ **Three Payment Methods:**
- `ETH-OnChain` – Native Ethereum
- `ETH-USDT-ERC20` – USDT stablecoin (6 decimals)
- `ETH-dEURO-ERC20` – dEURO euro-pegged token (1 dEURO = 1 EUR)

✅ **External RPC Integration** – No need to run your own Ethereum node

✅ **Automatic Payment Detection** – Background service scans blockchain for incoming payments

✅ **Configurable Confirmations** – Set required block confirmations per store

✅ **Store-Level Settings** – Each store can configure its own Ethereum receiving address

## Requirements

- **BTCPayServer:** v1.12.0 or later
- **.NET SDK:** 8.0
- **Ethereum RPC Endpoint:** Public or private (e.g., https://public-eth.nownodes.io, Infura, Alchemy, or your own node)

## Installation

### Option 1: Local Development

1. **Clone BTCPay Server with submodules:**
   ```bash
   git clone --recurse-submodules https://github.com/btcpayserver/btcpayserver.git
   cd btcpayserver


# Clone this plugin:

cd ..
git clone https://github.com/yourusername/BTCPayServer.Plugins.EthereumPayments.git
cd BTCPayServer.Plugins.EthereumPayments
git submodule update --init --recursive

# Add plugin to BTCPay solution:

cd ../btcpayserver
dotnet sln add ../BTCPayServer.Plugins.EthereumPayments/BTCPayServer.Plugins.EthereumPayments.csproj -s Plugins

# Build BTCPayServer
dotnet build BTCPayServer

# Build the plugin:

cd ../BTCPayServer.Plugins.EthereumPayments
dotnet build

# Run BTCPayServer:
cd ../btcpayserver/BTCPayServer
dotnet run

# Access plugin settings:

Navigate to: Store Settings > Integrations > Ethereum Payments

Configure RPC URL and receiving address

# Option 2: Plugin Builder (Production)
- Upload plugin repository to Plugin Builder: https://plugin-builder.btcpayserver.org/
- Build for your BTCPay Server version
- Download and install via BTCPay Admin > Plugins > Upload

# Configuration
Store Settings
Navigate to: Store → Plugins → Ethereum Settings

| Setting            | Description                        | Default                                    |
| ------------------ | ---------------------------------- | ------------------------------------------ |
| RPC URL            | Ethereum JSON-RPC endpoint         | https://public-eth.nownodes.io             |
| Receiving Address  | Your Ethereum EOA address (0x...)  | (required)                                 |
| USDT Contract      | USDT ERC-20 contract address       | 0xdAC17F958D2ee523a2206206994597C13D831ec7 |
| dEURO Contract     | dEURO ERC-20 contract address      | 0x643B1C5F2eA59A6868e1C8e8B51Bf37d52E76A5F |
| Confirmation Count | Blocks to wait before marking paid | 12                                         |
| Scan Interval      | Seconds between blockchain scans   | 15                                         |


# Enable Payment Methods
After configuring settings:

1) Go to Store → Checkout Experience → Payment Methods

2) Enable:
  - ETH-OnChain
  - ETH-USDT-ERC20
  - ETH-dEURO-ERC20

# How It Works
## Payment Flow
1) Customer creates invoice → BTCPayServer generates invoice with selected payment methods
2) Checkout displays → Shows Ethereum address and amount (ETH, USDT, or dEURO)
3) Customer sends payment → Transfers funds to displayed address
4) Background scanner detects → Monitors blockchain for incoming transactions every 15 seconds
5) Confirmations accumulate → Waits for configured block confirmations (default: 12)
6) Invoice marked paid → Payment confirmed and order completes

## Payment Detection
- ETH: Scans transaction recipients in recent blocks
- ERC-20 (USDT/dEURO): Monitors Transfer events using event logs
- Tolerance: 1% over/underpayment accepted

# MVP Limitations & Roadmap
## Current Limitations (MVP v1.0)
❌ Single address per store – All invoices use same receiving address
❌ No HD derivation – Cannot generate unique address per invoice
❌ Manual gas management – Merchants must handle gas fees manually
❌ External RPC dependency – Relies on third-party Ethereum node

## Planned Features (v2.0+)
🔜 HD address derivation – Unique address per invoice (BIP-44 derivation)
🔜 ERC-4337 Smart Accounts – Use Account Abstraction for receiving payments
🔜 Paymaster integration – Centralized gas fee sponsorship
🔜 Multi-network support – Polygon, Arbitrum, Optimism, Base
🔜 Refund functionality – Send refunds back to customer addresses
🔜 Webhook notifications – Real-time payment alerts

# Development
## Project Structure

BTCPayServer.Plugins.EthereumPayments/
├── Models/              # Data models (settings, payment details)
├── PaymentMethods/      # Payment method handlers (ETH, ERC-20)
├── Services/            # RPC service, scanner, background watcher
├── Controllers/         # UI controller for settings page
├── Views/               # Razor views for settings UI
└── Plugin.cs            # Main plugin entry point


## Dependencies
Nethereum.Web3 – Ethereum .NET integration library

Nethereum.Contracts – Smart contract interaction

Nethereum.RPC – JSON-RPC client

## Debugging
Enable detailed logging in appsettings.json:
{
  "Logging": {
    "LogLevel": {
      "BTCPayServer.Plugins.EthereumPayments": "Debug"
    }
  }
}


# Testing
Use Testnet (Sepolia):

Change RPC URL to Sepolia endpoint

Use Sepolia testnet USDT/dEURO contracts

Get free testnet ETH from faucet

Create test invoice:
- Select ETH payment method
- Send small amount to displayed address
- Monitor logs for payment detection

# Security Considerations
⚠️ Important Security Notes:

Private Key Security:
- Never store private keys in plugin code
- Use hardware wallets or secure key management
- Plugin only monitors receiving address (read-only)

RPC Endpoint Security:
- Use trusted RPC providers (Infura, Alchemy, or own node)
- Consider rate limits and API key rotation
- Monitor RPC endpoint availability

Address Validation:
- Always verify receiving address configuration
- Test with small amounts first
- Use checksummed addresses

Production Recommendations:
- Run your own Ethereum node for reliability
- Use mainnet for production, testnet for testing
- Implement monitoring and alerting
- Regular security audits

# Support
- Documentation: BTCPay Plugin Docs
- Community: BTCPay Slack
- Issues: GitHub Issues








