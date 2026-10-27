using System;
using System.Collections.Generic;

namespace AI_CRM.AI.Models
{
    public class ChatMessageItem
    {
        public string Role { get; set; } // "user" or "model"
        public string Content { get; set; }
    }

    public class CustomerAiContext
    {
        public string CompanyName { get; set; }
        public string FacilityCode { get; set; }
        public string HospitalLevel { get; set; }
        public string HospitalClass { get; set; }
        public int? BedCount { get; set; }
        public int? DailyOutpatients { get; set; }
        public string CurrentSoftware { get; set; }
        public string OwnershipType { get; set; }
        public int ActiveContractsCount { get; set; }
        public decimal TotalContractValue { get; set; }
        public int ActiveProjectsCount { get; set; }
        public int OpenTicketsCount { get; set; }
        public string RecentInteractions { get; set; }
    }

    public class TicketAiContext
    {
        public int TicketId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CustomerName { get; set; }
        public string Priority { get; set; }
        public string CurrentSoftware { get; set; }
        public string RelevantKnowledgeBase { get; set; }
    }

    public class AiAnalysisResult
    {
        public bool IsSuccess { get; set; }
        public string Content { get; set; }
        public string ErrorMessage { get; set; }
    }
}
