﻿using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IApiDataConnector
    {
        Task<TApiModel?> GetAsync<TApiModel>(HttpClient? httpClient, Uri url)
            where TApiModel : class;
    }
}