using System;
using System.Net.Http;
using System.Threading.Tasks;
using EasyCaching.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace teste_dotnet.Controllers
{
  [Serializable]
  public class addressViaCepDto
  {
    public string Cep { get; set; }
    public string Logradouro { get; set; }
    public string Complemento { get; set; }
    public string Bairro { get; set; }
    public string Localidade { get; set; }
    public string Uf { get; set; }
  }

  [Serializable]
  public class addressDto
  {
    public string Cep { get; set; }
    public string Address { get; set; }
    public string Complement { get; set; }
    public string Neighborhood { get; set; }
    public string Locality { get; set; }
    public string Uf { get; set; }
  }

  [ApiController]
  [Route("cep/")]
  public class ListCep : ControllerBase
  {
    static HttpClient HttpClient = new HttpClient();
    private IEasyCachingProvider cachingProvider;
    private IEasyCachingProviderFactory cachingProviderFactory;

    public ListCep(IEasyCachingProviderFactory cachingProviderFactory)
    {
      this.cachingProviderFactory = cachingProviderFactory;
      this.cachingProvider = this.cachingProviderFactory.GetCachingProvider("redis1");
    }
    [HttpGet]
    [Route("{cep:float}")]
    public async Task<addressDto> GetAddressByCep(float cep)
    {
      var itemCache = this.cachingProvider.Get<addressDto>(cep.ToString());
      if (!itemCache.HasValue)
      {
        HttpResponseMessage response = HttpClient.GetAsync(string.Format($"https://viacep.com.br/ws/{cep}/json/")).Result;
        string responseBody = await response.Content.ReadAsStringAsync();

        addressViaCepDto payloadAddress = JsonConvert.DeserializeObject<addressViaCepDto>(responseBody);
        addressDto payload = new addressDto
        {
          Cep = payloadAddress.Cep,
          Address = payloadAddress.Logradouro,
          Complement = payloadAddress.Complemento,
          Neighborhood = payloadAddress.Bairro,
          Locality = payloadAddress.Localidade,
          Uf = payloadAddress.Uf,
        };

        this.cachingProvider.Set<addressDto>(cep.ToString(), payload, TimeSpan.FromMinutes(1));

        return payload;
      }

      return itemCache.Value;
    }
  }
}