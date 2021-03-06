﻿using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Pokedex.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pokedex.Core.ViewModels
{
    public class PokedexViewModel : ParentViewModel
    {
        public string _pokemonName;
        public string PokemonName
        {
            set => SetProperty(ref _pokemonName, value);
            get => _pokemonName;
        }

        public string _pokemonUrl;
        public string PokemonUrl
        {
            set => SetProperty(ref _pokemonUrl, value);
            get => _pokemonUrl;
        }

        private IRepository _repository;
        public IMvxAsyncCommand GetPokemonsCommand { private set; get; }
        public IMvxCommand<string> SearchPokemonCommand { private set; get; }
        public IMvxCommand GetCachePokemons { private set; get; }
        public IMvxAsyncCommand<Models.Entities.Pokemon> PokemonSelectedCommand { private set; get; }


        public MvxObservableCollection<Models.Entities.Pokemon> Pokemons { private set; get; }
        public MvxObservableCollection<Models.Entities.Pokemon> PokemonsCache { private set; get; }

        public PokedexViewModel(IMvxNavigationService navigationService, IRepository repository) : base(navigationService)
        {
            Title = "Pokedex";
            _repository = repository;
            GetPokemonsCommand = new MvxAsyncCommand(GetPokemons);
            SearchPokemonCommand = new MvxCommand<string>((word) => SearchPokemon(word));
            Pokemons = new MvxObservableCollection<Models.Entities.Pokemon>();
            PokemonsCache = new MvxObservableCollection<Models.Entities.Pokemon>();
            PokemonSelectedCommand = new MvxAsyncCommand<Models.Entities.Pokemon>(OnPokemonSelected);
        }

        private void SearchPokemon(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Pokemons.Clear();
                Pokemons.AddRange(PokemonsCache);
            } else 
            {
                Pokemons.Clear();
                var pokemonFounds = PokemonsCache.Where(pokemon => pokemon.name.Contains(name));
                Pokemons.AddRange(pokemonFounds);
            }
        }

        private async Task OnPokemonSelected(Models.Entities.Pokemon pokemon)
        {
            var pokemonDetail = await _repository.GetDataAsync<Models.Entities.PokemonDetail>(pokemon.url);
            await _navigationService.Navigate<PokemonDetailViewModel, Models.Entities.PokemonDetail>(pokemonDetail);
        }

        private async Task GetPokemons()
        {
            var pokedex = await _repository.GetDataAsync<Models.Entities.Pokedex>();

            if (pokedex == null)
            {
                return;
            }

            var pokemons = pokedex.results;
            if (pokemons == null && !pokemons.Any())
            {
                return;
            }

            Pokemons.AddRange(pokedex.results);
            PokemonsCache.AddRange(pokedex.results);
        }

        public override async Task Initialize()
        {
            await GetPokemons();
        }
    }
}
