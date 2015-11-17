using System;
using Microsoft.AspNet.SignalR;
using PlanningPoker.Models;

namespace PlanningPoker
{
    public class PokerHub : Hub
    {
        public void PlayerJoined(string gameId, string name)
        {
            var gid = Guid.Parse(gameId);

            var game = GameManager.GetPokerGame(gid);
            if (game != null)
            {
                Clients.Group(gameId).playerJoined(name);
            }
        }

        public void PlayerVoted(string gameId, string playerId, float? points)
        {
            var gid = Guid.Parse(gameId);
            var pid = Guid.Parse(playerId);

            var game = GameManager.GetPokerGame(gid);
            if (game != null)
            {
                var player = game.GetPlayer(pid);
                player.CurrentVote = points;
                Clients.Group(gameId).playerVoted(playerId, points);
            }
        }

        public void ClearVotes(string gameId)
        {
            var gid = Guid.Parse(gameId);

            var game = GameManager.GetPokerGame(gid);
            if (game != null)
            {
                game.ClearVotes(playerId => { Clients.Group(gameId).playerVoted(playerId.ToString(), null); });
            }
        }

        public void PlayerLeft(string gameId, string playerId)
        {
            var gid = Guid.Parse(gameId);
            var pid = Guid.Parse(playerId);

            var game = GameManager.GetPokerGame(gid);
            if (game != null)
            {
                game.RemovePlayer(pid);
                Clients.Group(gameId).playerLeft(playerId);
            }
        }

        public void JoinGroup(string groupName)
        {
            this.Groups.Add(this.Context.ConnectionId, groupName);
        }

        public void AddStory(string gameId, string title, string description)
        {
            var gid = Guid.Parse(gameId);

            var game = GameManager.GetPokerGame(gid);
            if (game != null)
            {
                var story = game.AddStory(title, description);
                Clients.Group(gameId).storyAdded(story);
            }
        }

        public void AcceptEstimate(string gameId, string storyId, float estimate)
        {
            var gid = Guid.Parse(gameId);
            var sid = Guid.Parse(storyId);

            var game = GameManager.GetPokerGame(gid);
            if (game != null)
            {
                var story = game.GetStory(sid);
                story.Estimate = estimate;
                ClearVotes(gameId);
                Clients.Group(gameId).estimateAccepted(storyId, estimate);
            }
        }
    }
}