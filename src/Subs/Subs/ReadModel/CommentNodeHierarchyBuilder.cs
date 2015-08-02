﻿using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Membership;
using Subs.Services;

namespace Subs.ReadModel
{
    public class CommentNodeHierarchyBuilder : ICommentNodeHierarchyBuilder
    {
        private readonly ICommentDao _commentDao;
        private readonly IMembershipService _membershipService;
        private readonly ISubDao _subDao;
        private readonly IPermissionDao _permissionDao;
        private readonly IVoteDao _voteDao;

        public CommentNodeHierarchyBuilder(ICommentDao commentDao, 
            IMembershipService membershipService, 
            ISubDao subDao, 
            IPermissionDao permissionDao,
            IVoteDao voteDao)
        {
            _commentDao = commentDao;
            _membershipService = membershipService;
            _subDao = subDao;
            _permissionDao = permissionDao;
            _voteDao = voteDao;
        }

        public List<CommentNode> Build(CommentTree tree, CommentTreeContext treeContext, User currentUser)
        {
            var wrapped = WrapComments(treeContext.Comments, currentUser);
            var final = new List<CommentNode>();

            foreach (var comment in wrapped.Values)
            {
                comment.NumberOfChildren = treeContext.CommentsChildrenCount[comment.Comment.Id];

                CommentNode parent = null;

                if (comment.Comment.ParentId.HasValue)
                    parent = wrapped.ContainsKey(comment.Comment.ParentId.Value)
                        ? wrapped[comment.Comment.ParentId.Value]
                        : null;

                if (parent != null && comment.CurrentUserIsAuthor)
                {
                    // this comment is the current user, so lets walk the parents
                    // and uncollapse any comments which may be collapsed
                    var ancestor = parent;
                    var walked = new List<Guid>();
                    while (ancestor != null && !walked.Contains(ancestor.Comment.Id))
                    {
                        ancestor.Collapsed = false;
                        walked.Add(ancestor.Comment.Id);
                        ancestor = (ancestor.Comment.ParentId.HasValue && wrapped.ContainsKey(comment.Comment.ParentId.Value))
                            ? wrapped[comment.Comment.ParentId.Value]
                            : null;
                    }
                }

                if (parent != null)
                    parent.Children.Add(comment);
                else
                    final.Add(comment);
            }

            return final;
        }

        private Dictionary<Guid,CommentNode> WrapComments(List<Guid> comments, User currentUser)
        {
            var result = _commentDao.GetCommentsByIds(comments).ToDictionary(comment => comment.Id, comment => new CommentNode {Comment = comment});

            // TODO: Thinking about future support for cassandra, we may want to store "user ids" instead of "user names"
            // inside of each comment. That way, we can query authors by id, which is VERY fast in cassandra.
            var authorNames = result.Values.Select(x => x.Comment.AuthorUserName).Distinct().ToList();
            var authors = _membershipService.GetUsersByUserNames(authorNames).ToDictionary(x => x.UserName, x => x);

            // TODO: Same thing as the note above about authors, but for subs.
            var subs = _subDao.GetSubByNames(result.Values.Select(x => x.Comment.SubName).Distinct().ToList()).ToDictionary(x => x.Name, x => x);

            var userCanModInSubs = new List<Guid>();

            if(currentUser != null)
                foreach (var sub in subs.Values)
                    // TODO: Check for a specific permission "ban".
                    if(_permissionDao.CanUserModerateSub(currentUser.UserName, sub.Name))
                        userCanModInSubs.Add(sub.Id);

            var likes = currentUser != null ? _voteDao.GetVotesOnCommentsByUser(currentUser.UserName, comments) : new Dictionary<Guid, VoteType>();

            foreach (var item in result.Values)
            {
                item.Author = authors.ContainsKey(item.Comment.AuthorUserName) ? authors[item.Comment.AuthorUserName] : null;
                item.CurrentUserVote = likes.ContainsKey(item.Comment.Id) ? likes[item.Comment.Id] : (VoteType?)null;
                item.Sub = subs.ContainsKey(item.Comment.SubName) ? subs[item.Comment.SubName] : null;
                item.Score = item.Comment.VoteUpCount - item.Comment.VoteDownCount;

                var userCanMod = item.Sub != null && userCanModInSubs.Contains(item.Sub.Id);

                // TODO: make this configurable per-user
                int minimumScore = 0;
                if ((item.Author != null && currentUser != null) && currentUser.Id == item.Author.Id)
                {
                    // the current user is the author, don't collapse!
                    item.Collapsed = false;
                    item.CurrentUserIsAuthor = true;
                }else if (item.Score < minimumScore)
                {
                    // too many down votes to show to the user
                    item.Collapsed = true;
                }
                else
                {
                    // the current user is not the author, and we have enough upvotes to display,
                    // don't collapse
                    item.Collapsed = false;
                }
                
                item.CanDelete = userCanMod || item.CurrentUserIsAuthor;
                item.CanEdit = item.CurrentUserIsAuthor;
            }

            return result;
        }
    }
}
