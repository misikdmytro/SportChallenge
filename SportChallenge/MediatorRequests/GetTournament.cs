﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SportChallenge.Core.DbContexts;
using SportChallenge.Core.DbContexts.Contracts;
using SportChallenge.Core.Models;
using SportChallenge.Core.Repositories;
using SportChallenge.Models;

namespace SportChallenge.MediatorRequests
{
    public class GetTournamentRequest : IRequest<TournamentModel>
    {
        public string TournamentName { get; }

        public GetTournamentRequest(string tournamentName)
        {
            TournamentName = tournamentName;
        }
    }

    public class GetTournamentHandler : IRequestHandler<GetTournamentRequest, TournamentModel>
    {
        private readonly IContextFactory<SportContext> _contextFactory;

        public GetTournamentHandler(IContextFactory<SportContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<TournamentModel> Handle(GetTournamentRequest request, CancellationToken cancellationToken)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var tournamentRepository = new DbRepository<Tournament>(context);
                var tournament = await tournamentRepository
                    .SingleOrDefaultAsync(x => x.Name == request.TournamentName);

                if (tournament == null)
                {
                    return null;
                }

                var gamesRepository = new GameRepsoitory(context);
                var games = await gamesRepository.GetTournamentGames(tournament.Id);

                return new TournamentModel
                {
                    TournamentName = tournament.Name,
                    Rounds = games.Select(x => x.Round).Distinct()
                        .Select(x => new RoundModel
                        {
                            Number = x.Number,
                            Games = x.Games.Select(g => new GameModel
                            {
                                HomeTeam = g.HomeTeam.Name,
                                GuestTeam = g.GuestTeam.Name,
                                Result = g.Result == null
                                    ? null
                                    : new ResultModel
                                    {
                                        Guest = g.Result.GuestTeamResult,
                                        Home = g.Result.HomeTeamResult
                                    }
                            }).ToArray()
                        })
                        .ToArray()
                };
            }
        }
    }
}
