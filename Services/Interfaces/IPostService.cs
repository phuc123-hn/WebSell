using System.Collections.Generic;
using web1.Models;

namespace web1.Services.Interfaces
{
    /// <summary>
    /// Post business logic service
    /// </summary>
    public interface IPostService
    {
        // Read
        List<Post> GetAllActive(int pageSize = 10, int pageNumber = 1);
        List<Post> GetByType(int type, int pageSize = 10, int pageNumber = 1);
        Post GetById(int id);
        int GetTotalCount();

        // Write
        bool CreatePost(Post post);
        bool UpdatePost(Post post);
        bool DeletePost(int postId);
        bool RestorePost(int postId);

        // Admin
        List<Post> GetAllIncludingDeleted(int pageSize = 10, int pageNumber = 1);
    }

    /// <summary>
    /// Comment business logic service
    /// </summary>
    public interface ICommentService
    {
        // Read
        List<Comment> GetByPostId(int postId);
        Comment GetById(int id);

        // Write
        bool AddComment(Comment comment);
        bool DeleteComment(int commentId);
    }

    /// <summary>
    /// Reaction business logic service
    /// </summary>
    public interface IReactionService
    {
        // Read
        dynamic GetReactionCounts(int postId);
        string GetUserReaction(int postId, int userId);

        // Write
        bool ToggleReaction(int postId, int userId, string reactionType);
    }

    /// <summary>
    /// User business logic service
    /// </summary>
    public interface IUserService
    {
        // Read
        User GetById(int id);
        User GetByUsername(string username);
        User GetByEmail(string email);

        // Write
        bool CreateUser(User user);
        bool UpdateUser(User user);
        bool DeleteUser(int userId);

        // Ban management
        bool BanUser(int userId, int days, string reason);
        bool UnbanUser(int userId);
        bool IsUserBanned(int userId);
    }
}
