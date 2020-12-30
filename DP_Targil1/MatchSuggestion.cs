﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FacebookWrapper.ObjectModel;
using DP_Targil1.Patterns.Builder;
using FacebookWrapper;

namespace DP_Targil1
{
    public class MatchSuggestion
    {
        private const int k_NoMatchPercentage = 0;
        private const int k_Params = 5;
        private FacebookUser m_CurrentUser;
        private List<FacebookUser> m_TopMatchedUsers = new List<FacebookUser>();
        private SortedList<string, FacebookUser> m_MatchPersons = new SortedList<string, FacebookUser>();

        public MatchSuggestion(User i_LoggedInUser)
        {
            FacebookUserComposer facebookUserComposer = new FacebookUserComposer();
            IUserBuilder userBuilder = new FacebookUserBuilder(i_LoggedInUser);
            facebookUserComposer.Construct(userBuilder);
            m_CurrentUser = facebookUserComposer.GetFacebookUser(userBuilder);
            InitMatchPersons();
        }

        public List<FacebookUser> TopMatchedUsers
        {
            get
            {
                return m_TopMatchedUsers;
            }
        }

        public void InitMatchPersons()
        {
            foreach (User myFriend in m_CurrentUser.FriendsCollection)
            {
                foreach (User matchPerson in myFriend.Friends)
                {
                    if (matchPerson.Id != m_CurrentUser.User.Id)
                    {
                        if (!m_MatchPersons.ContainsKey(matchPerson.Id))
                        {
                            m_MatchPersons.Add(matchPerson.Id, new FacebookUser(matchPerson));
                        }
                    }
                }
            }
        }

        private bool isMatching(FacebookUser i_User, string i_Gender, int i_FromAge, int i_ToAge, string i_Hometown)
        {
            bool isMatching = false;
            if (i_User.User.Gender == null && i_Gender != null)
            {
                isMatching = false;
            }
            else if (i_User.City == null && i_Hometown != null)
            {
                isMatching = false;
            }
            else
            {
                isMatching = (string.IsNullOrEmpty(i_Gender) || i_User.User.Gender.ToString() == i_Gender)
                && (string.IsNullOrEmpty(i_Hometown) || i_User.City.ToString() == i_Hometown)
                && (i_User.Age >= i_FromAge || i_FromAge == 0)
                && (i_User.Age <= i_ToAge || i_FromAge == 0);
            }

            return isMatching;
        }

        public void CheckPersonMatch(string i_Gender, int i_FromAge, int i_ToAge, string i_Hometown)
        {
            foreach (KeyValuePair<string, FacebookUser> matchPerson in m_MatchPersons)
            {
                if (isSingle(matchPerson))
                {
                    if (isMatching(matchPerson.Value, i_Gender, i_FromAge, i_ToAge, i_Hometown))
                    {
                        checkMatchingPercentage(matchPerson);
                        {
                            m_TopMatchedUsers.Add(matchPerson.Value);
                        }

                        matchPerson.Value.MatchPercentage += 100 / k_Params;
                    }
                    else
                    {
                        matchPerson.Value.MatchPercentage = k_NoMatchPercentage;
                    }
                }
            }
        }

        private void sortMatchingList()
        {
            m_TopMatchedUsers.Sort(comparisonByPercent);
        }

        private int comparisonByPercent(FacebookUser i_UserData1, FacebookUser i_UserData2)
        {
            return i_UserData2.MatchPercentage.CompareTo(i_UserData1.MatchPercentage);
        }

        private void checkMatchingPercentage(KeyValuePair<string, FacebookUser> i_User)
        {
            checkCityMatch();
            checkSchoolMatch();
            checkGroupsMatch();
            checkJobsMatch();
        }

        private void checkSchoolMatch()
        {
            foreach (KeyValuePair<string, FacebookUser> matchPerson in m_MatchPersons)
            {
                if (matchPerson.Value.SchoolsCollection != null && m_CurrentUser.SchoolsCollection != null)
                {
                    if (matchPerson.Value.SchoolsCollection.Count != 0 && m_CurrentUser.SchoolsCollection.Count != 0)
                    {
                        if (matchPerson.Value.SchoolsCollection.Last().Id == m_CurrentUser.SchoolsCollection.Last().Id)
                        {
                            matchPerson.Value.MatchPercentage += 100 / k_Params;
                        }
                    }
                }
            }
        }

        private void checkCityMatch()
        {
            foreach (KeyValuePair<string, FacebookUser> matchPerson in m_MatchPersons)
            {
                if (matchPerson.Value.City != null && m_CurrentUser.City != null)
                {
                    if (matchPerson.Value.City.Id == m_CurrentUser.City.Id)
                    {
                        matchPerson.Value.MatchPercentage += 100 / k_Params;
                    }
                }
            }
        }

        private void checkJobsMatch()
        {
            foreach (KeyValuePair<string, FacebookUser> matchPerson in m_MatchPersons)
            {
                if (matchPerson.Value.JobsCollection != null && m_CurrentUser.JobsCollection != null)
                {
                    if (matchPerson.Value.JobsCollection.Count != 0 && m_CurrentUser.JobsCollection.Count != 0)
                    {
                        if (matchPerson.Value.JobsCollection.Last().Id == m_CurrentUser.JobsCollection.Last().Id)
                        {
                            matchPerson.Value.MatchPercentage += 100 / k_Params;
                        }
                    }
                }
            }
        }

        private void checkGroupsMatch()
        {
            foreach (KeyValuePair<string, FacebookUser> matchPerson in m_MatchPersons)
            {
                if (matchPerson.Value.GroupsCollection != null && m_CurrentUser.GroupsCollection != null)
                {
                    foreach (Group myGroup in m_CurrentUser.GroupsCollection)
                    {
                        foreach (Group friendGroup in matchPerson.Value.GroupsCollection)
                        {
                            if (myGroup.Id == friendGroup.Id)
                            {
                                matchPerson.Value.MatchPercentage += 100 / k_Params;
                            }
                        }
                    }
                }
            }
        }

        private bool isSingle(KeyValuePair<string, FacebookUser> i_User)
        {
            return i_User.Value.User.RelationshipStatus == User.eRelationshipStatus.Single || i_User.Value.User.RelationshipStatus == User.eRelationshipStatus.None;
        }
    }
}
