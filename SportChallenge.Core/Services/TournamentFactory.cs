﻿using System;
using System.Linq;
using System.Threading.Tasks;
using SportChallenge.Core.DbContexts;
using SportChallenge.Core.DbContexts.Contracts;
using SportChallenge.Core.Models;
using SportChallenge.Core.Repositories;
using SportChallenge.Core.Services.Contracts;

namespace SportChallenge.Core.Services
{
    public class TournamentFactory : ITournamentFactory
    {
        private readonly IContextFactory<SportContext> _contextFactory;

        public TournamentFactory(IContextFactory<SportContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Tournament> Create(string name, params string[] teamNames)
        {
            if (teamNames == null || teamNames.Length < 2)
            {
                throw new ArgumentException("Need team names", nameof(teamNames));
            }

            if (teamNames.GroupBy(x => x).Any(x => x.Count() > 1))
            {
                throw new ArgumentException("Team names in tournament should be unique", nameof(teamNames));
            }

            var tournament = new Tournament
            {
                Teams = teamNames.Select(x => new Team { Name = x }).ToList(),
                Name = name
            };

            using (var context = _contextFactory.CreateContext())
            {
                var repository = new DbRepository<Tournament>(context);
                await repository.AddAsync(tournament);

                return tournament;
            }
        }
    }
}
