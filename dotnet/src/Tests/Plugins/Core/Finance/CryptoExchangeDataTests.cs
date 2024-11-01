using Agience.Plugins.Core.Finance;
using FluentAssertions;

namespace Agience.Plugins.Core.Tests.Finance;

public class CryptoExchangeDataTests
{
    [Fact]
    public async Task GetCryptoTickerExchangeData_BitcoinPrice_ShouldBeInRange()
    {
        var cryptoExchangeData = new CryptoExchangeData();

        var bitcoinPrice = await cryptoExchangeData.GetCryptoTickerExchangeData("BTCUSDT");

        //Price reference on June 2024: 65,000 USD
        bitcoinPrice.Should().BeInRange(1000, 250000);
    }

    [Fact]
    public async Task GetCryptoTickerExchangeData_EtherPrice_ShouldBeInRange()
    {
        var cryptoExchangeData = new CryptoExchangeData();

        var etherPrice = await cryptoExchangeData.GetCryptoTickerExchangeData("ETHUSDT");

        //Price reference on June 2024: 35,000 USD
        etherPrice.Should().BeInRange(1000, 200000);
    }

    [Fact]
    public async Task GetCryptoTickerExchangeData_UnfoundTicker_ThrowError()
    {
        var cryptoExchangeData = new CryptoExchangeData();

        Func<Task> action = async () => { await cryptoExchangeData.GetCryptoTickerExchangeData("ZXCVBNMNB"); };

        await action.Should().ThrowAsync<Exception>().Where(e => e.Message.StartsWith("BadRequest"));
    }
}