﻿using Subs;
using Subs.ReadModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Skimur.Web.ViewModels
{
    public class CreateEditSubModel
    {
        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        // TODO: Request validation
        [DisplayName("Sidebar text")]
        public string SidebarText { get; set; }
        
        // TODO: Request validation
        [DisplayName("Submission text")]
        public string SubmissionText { get; set; }

        [DisplayName("Type")]
        public SubType SubType { get; set; }

        [DisplayName("Is default")]
        public bool? IsDefault { get; set; }

        [DisplayName("Show in /s/all?")]
        public bool InAll { get; set; }

        [DisplayName("18 or older content?")]
        public bool Nsfw { get; set; }

        public bool IsEditing { get; set; }
    }

    public class SubPostsModel
    {
        public SubWrapped Sub { get; set; }

        public PagedList<PostWrapped> Posts { get; set; }

        public PostsSortBy SortBy { get; set; }

        public PostsTimeFilter? TimeFilter { get; set; }

        public bool IsFrontpage { get; set; }

        public bool IsAll { get; set; }
    }

    public class SearchResultsModel
    {
        public string Query { get; set; }

        public SubWrapped LimitingToSub { get; set; }

        public PagedList<SubWrapped> Subs { get; set; }

        public PagedList<PostWrapped> Posts { get; set; }

        public PostsSearchSortBy SortBy { get; set; }

        public PostsTimeFilter? TimeFilter { get; set; }

        public SearchResultType? ResultType { get; set; }
    }

    public enum SearchResultType
    {
        Sub,
        Post
    }

    public class BannedUsersFromSub
    {
        public Sub Sub { get; set; }

        public PagedList<SubUserBanWrapped> Users { get; set; }

        public string Query { get; set; }

        public BanUserModel BanUser { get; set; }
    }

    public class BanUserModel
    {
        [Required]
        public string UserName { get; set; }

        public string ReasonPrivate { get; set; }

        public string ReasonPublic { get; set; }
    }
}