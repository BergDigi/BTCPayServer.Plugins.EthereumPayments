using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using System.Numerics;
using BTCPayServer.Plugins.EthereumPayments.Models;

namespace BTCPayServer.Plugins.EthereumPayments.Services;

public class EthereumRpcService
{
    private readonly ILogger<EthereumRpcService> _logger;
    private Web3? _web3;
    private string _currentRpcUrl = string.Empty;

    // ERC-20 Transfer event signature
    private const string TransferEventSignature = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";

    // Minimal ERC-20 ABI for Transfer events and balanceOf
    private const string Erc20Abi = @"[
        {
            'constant': true,
            'inputs': [{'name': '_owner', 'type': 'address'}],
            'name': 'balanceOf',
            'outputs': [{'name': 'balance', 'type': 'uint256'}],
            'type': 'function'
        },
        {
            'anonymous': false,
            'inputs': [
                {'indexed': true, 'name': 'from', 'type': 'address'},
                {'indexed': true, 'name': 'to', 'type': 'address'},
                {'indexed': false, 'name': 'value', 'type': 'uint256'}
            ],
            'name': 'Transfer',
            'type': 'event'
        }
    ]";

    public EthereumRpcService(ILogger<EthereumRpcService> logger)
    {
        _logger = logger;
    }

    public void ConfigureRpcUrl(string rpcUrl)
    {
        if (_currentRpcUrl != rpcUrl)
        {
            _web3 = new Web3(rpcUrl);
            _currentRpcUrl = rpcUrl;
            _logger.LogInformation($"Ethereum RPC configured: {rpcUrl}");
        }
    }

    private Web3 GetWeb3(string rpcUrl)
    {
        if (_web3 == null || _currentRpcUrl != rpcUrl)
        {
            ConfigureRpcUrl(rpcUrl);
        }
        return _web3!;
    }

    /// <summary>
    /// Get ETH balance for an address
    /// </summary>
    public async Task<decimal> GetEthBalanceAsync(string rpcUrl, string address)
    {
        try
        {
            var web3 = GetWeb3(rpcUrl);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(address);
            return Web3.Convert.FromWei(balance.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting ETH balance for {address}");
            throw;
        }
    }

    /// <summary>
    /// Get ERC-20 token balance for an address
    /// </summary>
    public async Task<decimal> GetErc20BalanceAsync(string rpcUrl, string tokenContract, string address, int decimals)
    {
        try
        {
            var web3 = GetWeb3(rpcUrl);
            var contract = web3.Eth.GetContract(Erc20Abi, tokenContract);
            var balanceOfFunction = contract.GetFunction("balanceOf");
            var balance = await balanceOfFunction.CallAsync<BigInteger>(address);
            
            return (decimal)balance / (decimal)Math.Pow(10, decimals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting ERC-20 balance for {address} on contract {tokenContract}");
            throw;
        }
    }

    /// <summary>
    /// Get latest block number
    /// </summary>
    public async Task<ulong> GetLatestBlockNumberAsync(string rpcUrl)
    {
        try
        {
            var web3 = GetWeb3(rpcUrl);
            var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            return (ulong)blockNumber.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest block number");
            throw;
        }
    }

    /// <summary>
    /// Scan ETH transfers to a specific address within a block range
    /// </summary>
    public async Task<List<EthTransfer>> ScanEthTransfersAsync(
        string rpcUrl,
        string toAddress,
        ulong fromBlock,
        ulong toBlock)
    {
        try
        {
            var web3 = GetWeb3(rpcUrl);
            var transfers = new List<EthTransfer>();

            for (ulong blockNum = fromBlock; blockNum <= toBlock; blockNum++)
            {
                var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(
                    new BlockParameter(blockNum));

                if (block?.Transactions == null) continue;

                foreach (var tx in block.Transactions)
                {
                    if (tx.To != null && 
                        string.Equals(tx.To, toAddress, StringComparison.OrdinalIgnoreCase) &&
                        tx.Value?.Value > 0)
                    {
                        transfers.Add(new EthTransfer
                        {
                            TransactionHash = tx.TransactionHash,
                            From = tx.From,
                            To = tx.To,
                            Value = Web3.Convert.FromWei(tx.Value.Value),
                            BlockNumber = (ulong)tx.BlockNumber.Value,
                            Timestamp = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value).DateTime
                        });
                    }
                }
            }

            return transfers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error scanning ETH transfers to {toAddress}");
            throw;
        }
    }

    /// <summary>
    /// Scan ERC-20 Transfer events for a specific recipient address
    /// </summary>
    public async Task<List<Erc20Transfer>> ScanErc20TransfersAsync(
        string rpcUrl,
        string tokenContract,
        string toAddress,
        int decimals,
        ulong fromBlock,
        ulong toBlock)
    {
        try
        {
            var web3 = GetWeb3(rpcUrl);
            var transfers = new List<Erc20Transfer>();

            // Create event filter for Transfer events to our address
            var transferEventHandler = web3.Eth.GetEvent<TransferEventDTO>(tokenContract);
            
            var filterInput = transferEventHandler.CreateFilterInput(
                new[] { toAddress },
                new BlockParameter(fromBlock),
                new BlockParameter(toBlock)
            );

            var logs = await transferEventHandler.GetAllChangesAsync(filterInput);

            foreach (var log in logs)
            {
                transfers.Add(new Erc20Transfer
                {
                    TransactionHash = log.Log.TransactionHash,
                    From = log.Event.From,
                    To = log.Event.To,
                    Value = (decimal)log.Event.Value / (decimal)Math.Pow(10, decimals),
                    BlockNumber = (ulong)log.Log.BlockNumber.Value,
                    TokenContract = tokenContract
                });
            }

            return transfers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error scanning ERC-20 transfers to {toAddress} on contract {tokenContract}");
            throw;
        }
    }

    /// <summary>
    /// Get transaction confirmation count
    /// </summary>
    public async Task<int> GetTransactionConfirmationsAsync(string rpcUrl, string txHash)
    {
        try
        {
            var web3 = GetWeb3(rpcUrl);
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
            
            if (receipt == null || receipt.BlockNumber == null)
                return 0;

            var currentBlock = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var confirmations = (int)(currentBlock.Value - receipt.BlockNumber.Value);
            
            return confirmations > 0 ? confirmations : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting confirmations for tx {txHash}");
            return 0;
        }
    }

    // DTOs for Nethereum event handling
    [Nethereum.ABI.FunctionEncoding.Attributes.Event("Transfer")]
    public class TransferEventDTO : Nethereum.ABI.FunctionEncoding.Attributes.IEventDTO
    {
        [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("address", "from", 1, true)]
        public string From { get; set; } = string.Empty;

        [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("address", "to", 2, true)]
        public string To { get; set; } = string.Empty;

        [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("uint256", "value", 3, false)]
        public BigInteger Value { get; set; }
    }
}

// Transfer models
public class EthTransfer
{
    public string TransactionHash { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public ulong BlockNumber { get; set; }
    public DateTime Timestamp { get; set; }
}

public class Erc20Transfer
{
    public string TransactionHash { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public ulong BlockNumber { get; set; }
    public string TokenContract { get; set; } = string.Empty;
}
