using System;
using System.Collections.Generic;
using System.Linq;
using web1.Data;
using web1.Models;
using web1.Repositories.Interfaces;
using web1.Services.Interfaces;

namespace web1.Services.Implementations
{
    /// <summary>
    /// Post service implementation
    /// </summary>
    public class PostService : IPostService
    {
        private readonly IRepository<Post> _postRepository;

        public PostService(IRepository<Post> postRepository)
        {
            _postRepository = postRepository;
        }

        public List<Post> GetAllActive(int pageSize = 10, int pageNumber = 1)
        {
            return _postRepository
                .GetAll(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public List<Post> GetByType(int type, int pageSize = 10, int pageNumber = 1)
        {
            return _postRepository
                .GetAll(p => p.Type == type && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public Post GetById(int id)
        {
            return _postRepository.GetById(id);
        }

        public int GetTotalCount()
        {
            return _postRepository.Count(p => !p.IsDeleted);
        }

        public bool CreatePost(Post post)
        {
            try
            {
                post.CreatedAt = DateTime.Now;
                post.IsDeleted = false;
                _postRepository.Add(post);
                _postRepository.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdatePost(Post post)
        {
            try
            {
                post.UpdatedAt = DateTime.Now;
                _postRepository.Update(post);
                _postRepository.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeletePost(int postId)
        {
            try
            {
                var post = _postRepository.GetById(postId);
                if (post == null) return false;
                post.IsDeleted = true;
                _postRepository.Update(post);
                _postRepository.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RestorePost(int postId)
        {
            try
            {
                var post = _postRepository.GetById(postId);
                if (post == null) return false;
                post.IsDeleted = false;
                _postRepository.Update(post);
                _postRepository.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<Post> GetAllIncludingDeleted(int pageSize = 10, int pageNumber = 1)
        {
            return _postRepository
                .GetAll()
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }

    /// <summary>
    /// Comment service implementation
    /// </summary>
    public class CommentService : ICommentService
    {
        private readonly IRepository<Comment> _commentRepository;

        public CommentService(IRepository<Comment> commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public List<Comment> GetByPostId(int postId)
        {
            return _commentRepository
                .GetAll(c => c.PostId == postId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        public Comment GetById(int id)
        {
            return _commentRepository.GetById(id);
        }

        public bool AddComment(Comment comment)
        {
            try
            {
                comment.CreatedAt = DateTime.Now;
                comment.IsDeleted = false;
                _commentRepository.Add(comment);
                _commentRepository.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteComment(int commentId)
        {
            try
            {
                var comment = _commentRepository.GetById(commentId);
                if (comment == null) return false;
                comment.IsDeleted = true;
                _commentRepository.Update(comment);
                _commentRepository.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Reaction service implementation
    /// </summary>
    public class ReactionService : IReactionService
    {
        private readonly IRepository<Reaction> _reactionRepository;
        private readonly ApplicationDbContext _context;

        public ReactionService(IRepository<Reaction> reactionRepository, ApplicationDbContext context)
        {
            _reactionRepository = reactionRepository;
            _context = context;
        }

        public dynamic GetReactionCounts(int postId)
        {
            var reactions = _reactionRepository.Where(r => r.PostId == postId);
            return new
            {
                like = reactions.Count(r => r.ReactionType == "like"),
                love = reactions.Count(r => r.ReactionType == "love"),
                haha = reactions.Count(r => r.ReactionType == "haha"),
                wow = reactions.Count(r => r.ReactionType == "wow"),
                angry = reactions.Count(r => r.ReactionType == "angry"),
                total = reactions.Count()  // ? Call Count()
            };
        }

        public string GetUserReaction(int postId, int userId)
        {
            var reaction = _reactionRepository.FirstOrDefault(r => r.PostId == postId && r.UserId == userId);
            return reaction?.ReactionType;
        }

        public bool ToggleReaction(int postId, int userId, string reactionType)
        {
            try
            {
                var existing = _reactionRepository.FirstOrDefault(r => r.PostId == postId && r.UserId == userId);

                if (existing != null)
                {
                    if (existing.ReactionType == reactionType)
                    {
                        _reactionRepository.Delete(existing);
                    }
                    else
                    {
                        existing.ReactionType = reactionType;
                        existing.CreatedAt = DateTime.Now;
                        _reactionRepository.Update(existing);
                    }
                }
                else
                {
                    var reaction = new Reaction
                    {
                        PostId = postId,
                        UserId = userId,
                        ReactionType = reactionType,
                        CreatedAt = DateTime.Now
                    };
                    _reactionRepository.Add(reaction);
                }

                _reactionRepository.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
