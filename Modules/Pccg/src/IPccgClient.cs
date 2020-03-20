namespace Pccg
{
    using System;
    using System.Threading.Tasks;
    using FluentResults;
    using Pccg.Models;

    internal interface IPccgClient : IDisposable
    {
        Task<Result<Models.Version>> GetServerVersion();

        Task<Result<Card>> GetRandomCardFromCompendium();

        Task<Result<Card>> AddCardToCompendium(Card cardToAdd);
    }
}