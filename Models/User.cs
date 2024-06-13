using System.Text;
using ProjectBoard.Auth;

namespace ProjectBoard.Models;

class User {
    public int Id { get; set;}
    public string Username { get; set;}
    public string Password { get; set;}
    public string Hash { get; set; }

    public User(string username, string password) {
        // Generate a secure Id rather than the default incrementing ones from the DB.
        this.Id = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000);
        this.Username = username;
        this.Password = password;

        // Formate the username and password into bytes and then get hash.
        var credentialsBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        var credentialsBytes = Convert.FromBase64String(credentialsBase64);
        Hash = BasicAuthHandler.SharedHashAlgorithm(credentialsBytes);
    }
}