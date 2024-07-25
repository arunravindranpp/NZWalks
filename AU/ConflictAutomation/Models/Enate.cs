
using Nest;
using Org.BouncyCastle.Bcpg;
using PACE.Domain.Models;
using System;
using System.Diagnostics.Metrics;
using System.Net;
using System.Reflection.Emit;
using System.Xml.Linq;
using static PACE.Domain.Services.StartAssessmentService;

namespace ConflictAutomation.Models
{

    public class EnateCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class CasePacket2
    {
        public string CaseAttributeVersionGUID { get; set; }
     //   public string RequesterUserGUID { get; set; }
      //  public string LinkToPacketGUID { get; set; }
      //  public string CopyWorkItemDataType = "None";
     //   public string SchedulePeriodGUID { get; set; }
     // //  public bool Problem = true;
        public List<User> KeepActionsWithUser { get; set; }
        public bool MoveToNextStep = true;
        public string Title { get; set; }
        public string Description { get; set; }
     //   public DateTime? OverrideDueDate { get; set; }
     ////   public DateTime? ScheduledFollowUpOn { get; set; }
     ////   public DateTime? WaitForMoreInformationUntil { get; set; }
     ////   public List<KeepWithUser> KeepWithUser { get; set; }
     ////   public bool CloseIfNoResponseReceived = true;
     //   public bool DoNotSendAutomatedEmailsToContacts = true;
     //   public int AffectedRecordCount = 0;
     //   //public List<string> DataFields { get; set; }
        public CheckInfo DataFields { get; set; }
     //   public DateTime MostRecentDataFieldsUpdatedTime { get; set; }
     //   public List<Files> Files { get; set; }
     //   public List<Hyperlinks> Hyperlinks { get; set; }
     //   public string OpenPacketActivityGUID { get; set; }
     //   public float OpenDurationMS { get; set; }
     //   public int? OpenManualDurationMS { get; set; }
     //   public List<Contacts> Contacts { get; set; }
     //   public List<Defects> Defects { get; set; }
        public Result Result { get; set; }
    }

    public class CasePacket
    {
        public string CaseAttributeVersionGUID { get; set; }
        public string RequesterUserGUID { get; set; }
        public string LinkToPacketGUID { get; set; }
        //  public string CopyWorkItemDataType = "None";
        public string SchedulePeriodGUID { get; set; }
        public bool Problem = true;
    //    public List<User> KeepActionsWithUser { get; set; }
        public bool MoveToNextStep = true;
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? OverrideDueDate { get; set; }
        //   public DateTime? ScheduledFollowUpOn { get; set; }
        //   public DateTime? WaitForMoreInformationUntil { get; set; }
        public List<KeepWithUser> KeepWithUser { get; set; }
        //   public bool CloseIfNoResponseReceived = true;
        public bool DoNotSendAutomatedEmailsToContacts = true;
        public int AffectedRecordCount = 0;
        //public List<string> DataFields { get; set; }
        public CheckInfo DataFields { get; set; }
        public DateTime MostRecentDataFieldsUpdatedTime { get; set; }
        public List<Files> Files { get; set; }
        public List<Hyperlinks> Hyperlinks { get; set; }
        public string OpenPacketActivityGUID { get; set; }
        public float OpenDurationMS { get; set; }
        public int? OpenManualDurationMS { get; set; }
        public List<Contacts> Contacts { get; set; }
        public List<Defects> Defects { get; set; }
        public Result Result { get; set; }
    }

    public class ResolveActionPacket
    {
        public string GUID { get; set; }
        public string Title { get; set; }
        public int AffectedRecordCount { get; set; }

        public string NotDoneSuccessfullyComment { get; set; }
    }

    public class ActionPacket
    {
        public string GUID { get; set; }
        public string Title { get; set; }
        public int AffectedRecordCount { get; set; }

    }

    public class ChangeUserPasswordPacket
    {
        public string NewPassword { get; set; }
        
    }

    public class UserAssignmentPacket
    {
        public bool AllowChangingExistingAssignment { get; set; }
        public string UserGUID { get; set; }
        public string PacketGUID { get; set; }
        public string Note { get; set; }
    }

    public class User
    {
        public string GUID { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public bool HasProfilePicture { get; set; }
        public string UserType = "None";
        public bool IsMe { get; set; }
        public bool Retired { get; set; }
    }

    public class KeepWithUser
    {
        public string GUID { get; set; }
    }

    public class Files
    {
        public string TemporaryFileGUID { get; set; }
        public string FileName { get; set; }
        public string Data { get; set; }
        public string Note { get; set; }
        public Tags Tags { get; set; }
        public string Source = "AttachedToPacket";
        public bool Retired = true;
        public string GUID { get; set; }
    }

    public class Tags
    {
        public bool Retired = true;
        public string GUID { get; set; }
    }

    public class Hyperlinks
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public string Note { get; set; }
        public List<Tags> Tags { get; set; }
        public bool Retired = true;
        public string GUID { get; set; }
    }

    public class Contacts
    {
        public DateTime AddedOn { get; set; }
        public List<Tags> Tags { get; set; }
        public bool Retired { get; set; }
        public string GUID { get; set; }
        public User AddedByUser { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public bool HasProfilePicture { get; set; }
        public string UserType = "None";
        public bool IsMe { get; set; }
    }

    public class Defects
    {
        public string Description { get; set; }
        public int AffectedRecordCount { get; set; }
        public bool Resolved = true;
        public List<DefectPartyAtFaultOption> DefectPartyAtFaultOption { get; set; }
        public List<DefectCategory> DefectCategory { get; set; }
        public bool Retired = true;
        public string GUID { get; set; }
        public StartedByPacket Packet { get; set; }
        public DateTime AddedOn { get; set; }
        public User AddedByUser { get; set; }
        public int LoggedOnStep { get; set; }

    }

    public class DefectPartyAtFaultOption
    {
        public string GUID { get; set; }
    }

    public class DefectCategory
    {
        public string GUID { get; set; }
    }


    public class CasePacket_Update
    {
     //   public string SchedulePeriodGUID { get; set; }
      //  public bool Problem = true;
     //   public List<User> KeepActionsWithUser { get; set; }
        public bool MoveToNextStep = true;
        public string Title { get; set; }
        public string Description { get; set; }
     //   public DateTime OverrideDueDate { get; set; }
    //    public DateTime ScheduledFollowUpOn { get; set; }
     //   public DateTime WaitForMoreInformationUntil { get; set; }
     //   public List<KeepWithUser> KeepWithUser { get; set; }
     //   public bool CloseIfNoResponseReceived = true;
        public bool DoNotSendAutomatedEmailsToContacts = true;
        public int AffectedRecordCount { get; set; }
        //public List<string> DataFields { get; set; }
        public CheckInfo DataFields { get; set; }
        public DateTime MostRecentDataFieldsUpdatedTime { get; set; }
        public List<Files> Files { get; set; }
        public List<Hyperlinks> Hyperlinks { get; set; }
        public string OpenPacketActivityGUID { get; set; }
        public string OpenDurationMS { get; set; }
        public int? OpenManualDurationMS { get; set; }
        public List<Contacts> Contacts { get; set; }
        public List<Defects> Defects { get; set; }

    }

    public class Action_Update
    {
        public string PeerReviewComment { get; set; }
        public string RobotRejectedReason { get; set; }
        public List<CheckList> Checklist { get; set; }
        public List<CheckList> PeerReviewerChecklist { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? OverrideDueDate { get; set; }
        public DateTime? ScheduledFollowUpOn { get; set; }
        public DateTime? WaitForMoreInformationUntil { get; set; }
     //   public List<KeepWithUser> KeepWithUser { get; set; }
     //   public bool CloseIfNoResponseReceived = true;
        public bool DoNotSendAutomatedEmailsToContacts = true;
        public int AffectedRecordCount = 0;
        //public List<string> DataFields { get; set; }
        public CheckInfo DataFields { get; set; }
        public DateTime MostRecentDataFieldsUpdatedTime { get; set; }
        public List<Files> Files { get; set; }
        public List<Hyperlinks> Hyperlinks { get; set; }
        public string OpenPacketActivityGUID { get; set; }
        public string OpenDurationMS { get; set; }
        public int? OpenManualDurationMS { get; set; }
        public List<Contacts> Contacts { get; set; }
        public List<Defects> Defects { get; set; }
    }

    public class CheckList
    {
        public string PeerReviewNote { get; set; }
        public string Result { get; set; }
        public string Note { get; set; }
        public string GUID { get; set; }
    }

    public class PeerReviewerChecklist
    {
        public string Result { get; set; }
        public string Note { get; set; }
        public string GUID { get; set; }
    }

    public class ProcessContext_Company
    {
        public string ParentCompany { get; set; }
        public int CompanyType { get; set; }
        public bool HasLiveTickets { get; set; }
        public bool HasLiveCases { get; set; }
        public bool Retired { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class ProcessContext_Contracts
    {
        public int TotalCount { get; set; }
        public int Offset { get; set; }
        public List<ProcessContext_CompanyItems> Items { get; set; }

    }

    public class ProcessContext_CompanyItems
    {
        public ProcessContext_CustomerCompany CustomerCompany { get; set; }
        public ProcessContext_SupplierCompany SupplierCompany { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class ProcessContext_CustomerCompany
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class ProcessContext_SupplierCompany
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class ProcessContext_Services
    {
        public string ContractGUID { get; set; }
        public bool HasLiveTickets { get; set; }
        public bool HasLiveCases { get; set; }
        public bool Retired { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class ProcessContext_Processes
    {
        public int TotalCount { get; set; }
        public int Offset { get; set; }
        public List<ProcessContext_ProcessItem> Items { get; set; }
    }

    public class ProcessContext_ProcessItem
    {
        public bool IsContactsMandatory { get; set; }
        public bool IsAvailableToAllUsers { get; set; }
        public int ProcessType { get; set; }
        public int VersionState { get; set; }
        public string AttributeVersionGuid { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class Work_GetMoreWork
    {
        public string GUID { get; set; }
        public string Reference { get; set; }
        public string Title { get; set; }
        public DateTime? DueDate { get; set; }
        public int Status { get; set; }
        public int ProcessType { get; set; }
        public string CustomerName { get; set; }
        public string ContractName { get; set; }
        public string ServiceName { get; set; }
        public string ProcessName { get; set; }
        public string ConflictCheckId { get; set; }
    }

    public class Contracts
    {
        List<Services> Services { get; set; }
        public string GUID { get; set; }
        public string Name { get; set; }
    }
    public class Services
    {
        List<Processess> Processess { get; set; }
        public string GUID { get; set; }
        public string Name { get; set; }
    }
    public class Processess
    {
        public string AttributeGUID { get; set; }
        public string VersionState { get; set; }
        public string GUID { get; set; }
        public string Name { get; set; }
    }

    public class SetToDo
    {
        public List<EnateMessages> Messages { get; set; }

    }

    public class Result
    {
     //   public List<User> KeepActionsWithUser { get; set; }
     //   public DateTime? FeedbackWindowOpenUntil { get; set; }
        public List<EnateActions> Actions { get; set; }
        public List<PendingActions> PendingActions { get; set; }
        public int ManuallyStartableActionCount = 0;
        public bool HasSubCases = true;
        public bool HasLinkedPackets = true;
        public bool IsCaseReopenAllowed = true;
        public bool IsContactsMandatory = true;
        public string SchedulePeriodGUID { get; set; }
        public string WaitingFor = "Awaiting3rdParty";
        public bool Problem = false;
        public bool AllStepsAndActionsCompleted = true;
        public CaseAttributeVersion CaseAttributeVersion { get; set; }
        public CaseStep CaseStep { get; set; }
        public List<CaseStep> CaseSteps { get; set; }
        public SchedulePeriod SchedulePeriod { get; set; }
        public string EndDate { get; set; }
        public string CustomerGUID { get; set; }
        public string ContractGUID { get; set; }
        public string ServiceGUID { get; set; }
        public string ProcessAttributeObjectGUID { get; set; }
        public string ServiceLineGUID { get; set; }
        public DateTime? NewInformationReceivedOn { get; set; }
        public string TimeRemainingWhenPaused { get; set; }
        public DateTime LastUpdatedByUserOn { get; set; }
        public string CurrentQueueName { get; set; }
        public string StatusReason = "NewlyCreated";
        public string WaitType = "MoreInformation";
        public string ResolutionMethod = "CommunicationWithServiceRecipient";
      //  public User AssignedToUser { get; set; }
      //  public User OwnerUser { get; set; }
      //  public User StartedByUser { get; set; }
      //  public User WaitingForUser { get; set; }
      //  public User ResolvedByUser { get; set; }
    //    public User LastUpdatedByUser { get; set; }
      //  public StartedByPacket StartedByPacket { get; set; }
      //  public DateTime UpdatedTime { get; set; }
     //   public List<Files> Files { get; set; }
     //   public List<Hyperlinks> Hyperlinks { get; set; }
        public string Description { get; set; }
     //   public DateTime? OverrideDueDate { get; set; }
     //   public DateTime? ScheduledFollowUpOn { get; set; }
     //   public DateTime? WaitForMoreInformationUntil { get; set; }
      //  public User KeepWithUser { get; set; }
     //   public bool CloseIfNoResponseReceived = true;
        public bool DoNotSendAutomatedEmailsToContacts = true;
        public int AffectedRecordCount { get; set; }
       // List<string> DataFields = new List<string>();
       public CheckInfo DataFields { get; set; }
        public DateTime MostRecentDataFieldsUpdatedTime { get; set; }
        public string OpenPacketActivityGUID { get; set; }
        public float OpenDurationMS { get; set; }
        public int? OpenManualDurationMS { get; set; }
        public List<Defects> Defects { get; set; }
        public StartedByPacket ParentPacket { get; set; }
        public string GUID { get; set; }
        public CurrentPacketStatusHistory CurrentPacketStatusHistory { get; set; }
        public bool IsFollowed = true;
        public string StartDate { get; set; }
        public string CustomerName { get; set; }
        public string SupplierName { get; set; }
        public string ContractName { get; set; }
        public string ServiceName { get; set; }
        public string ServiceLineName { get; set; }
     //   public string ProcessTypeName { get; set; }
        public string CheckProcess { get; set; }
        public string Reference { get; set; }
        public string Title { get; set; }
        public string Status = "Draft";
        public string DueDate { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public string RAGStatus = "DueToday";
        public string ProcessType = "Case";
        public string StartedByMethod = "ByWorkflow";
    }

    public class EnateMessages
    {
        public string MessageID = "General_UnexpectedFailure";
        public bool IsError { get; set; }
        public string MessageDescription { get; set; }
        public List<string> Parameters { get; set; }
    }

    public class EnateActions
    {
     //   public DateTime? FeedbackWindowOpenUntil { get; set; }
        public ActionAttribute ActionAttribute { get; set; }
        public ActionGeneralAttribute ActionGeneralAttribute { get; set; }
        public DateAttribute DueDateAttribute { get; set; }
        public DateAttribute PeerReviewDueDateAttribute { get; set; }
        public string RobotRejectedReason { get; set; }
        public int RobotGetMoreWorkCount { get; set; }
        public User ResolvedByUser { get; set; }
        public User AssignedToUser { get; set; }
        public User OwnerUser { get; set; }
        public User PeerReviewedByUser { get; set; }

        public string CurrentQueueName { get; set; }
        public string Instruction { get; set; }
        public string TimeRemainingWhenPaused { get; set; }
        public bool InPeerReview = true;
        public string WaitType = "MoreInformation";
        public string ResolutionMethod = "CommunicationWithServiceRecipient";
        public string ActionSubType = "ManualAction";
        public DateTime StartDate { get; set; }
        public string CustomerName { get; set; }
        public string SupplierName { get; set; }
        public string ContractName { get; set; }
        public string ServiceName { get; set; }
        public string ServiceLineName { get; set; }
        public string ProcessTypeName { get; set; }
        public string Reference { get; set; }
        public string Title { get; set; }
        public string Status = "Draft";
        public DateTime DueDate { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public string RAGStatus = "DueToday";
        public string ProcessType = "Case";
        public string StartedByMethod = "ByWorkflow";
        public string GUID { get; set; }

    }

    public class ActionAttribute
    {
        public int Order { get; set; }
        public int StepNumber { get; set; }
        public bool AllowManualCreation { get; set; }
        public bool AutoClose { get; set; }
        public string GUID { get; set; }
        public string PreviousActionAttributeGUID { get; set; }
        public string PreviousCaseConditionOutputAttributeGUID { get; set; }
        public EnateActionType ActionType { get; set; }
        public string RemindBeforeDueDate { get; set; }
        public string RemindAfterDueDate { get; set; }
        public string ResponseEmailFrom { get; set; }
        public string EmailTo { get; set; }
        public string EmailCC { get; set; }
        public string EmailBCC { get; set; }
        public string OptionalEmailTo { get; set; }
        public string OptionalEmailCC { get; set; }
        public string OptionalEmailBCC { get; set; }
        public EmailTemplate EmailTemplate { get; set; }
        public ActionGeneralAttribute ActionGeneralAttribute { get; set; }
        public DateAttribute DueDateAttribute { get; set; }
        public DateAttribute AllocationAttribute { get; set; }
        public DateAttribute PeerReviewAllocationAttribute { get; set; }
        public DateAttribute PeerReviewDueDateAttribute { get; set; }
        public DateAttribute MainCard { get; set; }
        public DateAttribute SideCard { get; set; }
        public StartOrWaitCaseAttribute StartOrWaitCaseAttribute { get; set; }
        public bool StartCaseAsSubCase = true;
        public Abyy AbbyyPlatform { get; set; }
        public Abyy AbbyyProject { get; set; }
        public Abyy AbbyyFileTag { get; set; }
        public Abyy AbbyyOutputFileTag { get; set; }
        public string WebAPIIntegrationURL { get; set; }
        public bool WebAPIIntegrationResponseExpected = true;
        public bool Retired { get; set; }
        public List<LocalChecklists> LocalChecklists { get; set; }
        public int WebAPIIntegrationResponseWindow = 0;
        public Action_Get_DataFields DataFields { get; set; }

        public int Status { get; set; }
    }
    public class ActionGeneralAttribute
    {
        public bool HideCaseLink { get; set; }
        public string SOPURL { get; set; }
        public string EstimatedDurationMS { get; set; }
        public float RobotEstimatedDurationMS { get; set; }
        public int RecordCount { get; set; }
        public bool Retired { get; set; }
        public bool InUseByLiveProcess { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
        public RobotFarm RobotFarm { get; set; }
        public EnateActionType ActionType { get; set; }
    }
    public class DateAttribute
    {
        public int AllowOverrideDueDate { get; set; }
        public bool Retired { get; set; }
        public bool InUseByLiveProcess { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class PendingActions
    {
        public List<ActionAttribute> ActionAttribute { get; set; }
        public string GUID { get; set; }

    }

    public class EnateActionType
    {
        public string SubType { get; set; }
        public bool Retired { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class EmailTemplate
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class RobotFarm
    {
        public string Technology = "UIPath";
        public bool Retired { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class StartOrWaitCaseAttribute
    {
        public EnateActionType CaseType { get; set; }
        public Service Service { get; set; }
        public bool ShowOnContactsPage = true;
        public string GUID { get; set; }
    }

    public class Service
    {
        public Contract Contract { get; set; }
    }
    public class Contract
    {
        public ProcessContext_CustomerCompany CustomerCompany { get; set; }
        public ProcessContext_SupplierCompany SupplierCompany { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class Abyy
    {
        public string Name { get; set; }
        public string GUID { get; set; }
    }
    public class LocalChecklists
    {
        public bool FollowGlobalChecklist = true;
        public string LinkedToGlobalChecklistGUID { get; set; }
        public int Order { get; set; }
        public bool Retired { get; set; }
        public string Description { get; set; }
        public bool IsPeerReviewerChecklist = true;
        public string GUID { get; set; }
    }

    public class CaseAttributeVersion
    {
        public string RecordCountBehaviour = "Hide";
        public int RecordCount = 0;
        public string StepProgressionMode = "Manual";
        public string SOPURL { get; set; }
        public DateAttribute DueDateAttribute { get; set; }
        public Schedule Schedule { get; set; }
        public CaseAttribute CaseAttribute { get; set; }
        public bool AllowTitleChange = true;
        public int Version = 0;
        public string VersionState = "Draft";
        public string VersionNotes { get; set; }
        public User SetLiveByUser { get; set; }
        public DateTime SetLiveOn { get; set; }
        public string GUID { get; set; }
    }

    public class Schedule
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }
    public class CaseAttribute
    { 
        public CaseType CaseType { get; set; }
        public string GUID { get; set; }
    }

    public class CaseType
    {
        public bool IsContactsMandatory = true;
        public string Description { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class CaseStep
    {
        public int StepNumber { get; set; }
        public int Milestone { get; set; }
        public bool Retired = true;
        public string Name { get; set; }
        public string GUID { get; set; }
    }

    public class SchedulePeriod
    {
        public int Year { get; set; }
        public string Period { get; set; }
        public string GUID { get; set; }
    }
    public class StartedByPacket
    {
        public string StartDate { get; set; }
        public string CustomerName { get; set; }
        public string SupplierName { get; set; }
        public string ContractName { get; set; }
        public string ServiceName { get; set; }
        public string ServiceLineName { get; set; }
        public string ProcessTypeName { get; set; }
        public string Reference { get; set; }
        public string Title { get; set; }
        public string Status = "Draft";
        public string DueDate { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public string RAGStatus = "DueToday";
        public string ProcessType = "Case";
        public string StartedByMethod = "ByWorkflow";
        public string GUID { get; set; }
    }

    public class CurrentPacketStatusHistory
    {
        public DateTime StartDate { get; set; }
        public User SetByUser { get; set; }

    }   
    public class CheckInfo : ICheckInfo
    {
        public string CheckId { get; set; }
        public string ResubmittedCheck { get; set; }
        public string EntityName { get; set; }
        public string AbortReason { get; set; } = "";
        public string CheckProcess { get; set; }
        public string CheckIfSplitCasesCreatedOrNot { get; set; } = "No";
        public string ClientSideCheckOrNoRelationshipWithAnyOfTheCp { get; set; } = "Other – relationship with CPs";
        public string ClientSideCheckOrNoRelationshipWithAnyOfTheCpSManual { get; set; } = "";
        public int ActualEntitySCount { get; set; } = 0;
        public string AdditionalSort { get; set; } = "";
        public string BasedOnCompletedPursuitCheck { get; set; } = "No";
        public string ChecklistForCannedText { get; set; } = ""; //here
        public string CheckType { get; set; }
     //   public string ClientSideCheckOrNoRelationshipWithAnyOfTheCp { get; set; } = "";
     //   public string ClientSideCheckOrNoRelationshipWithAnyOfTheCpSManual { get; set; } = "";
        public string CommentsForUnableToComplete { get; set; } = "";
        public bool CompetitiveSituation { get; set; } = false;
        public string Confidentiality { get; set; } = "No";
        public bool ConflictConditions { get; set; } = false;
        public bool ConflictOfInterest { get; set; } = false;
        public string ConsolidationFailed { get; set; } = "";
        public bool CounterPartiesMissingNotProvidedInPace { get; set; } = false;
        public bool CounterpartiesNotAddedInPaceApg { get; set; } = false;
        public string Country { get; set; }
        public bool CpReworkRequired { get; set; } = false;
        public int EntitySBot { get; set; } = 0;
        public int EntitySPaceApg { get; set; }
        public string G360 { get; set; } = "No";
        public string GcspConsultants { get; set; } = "";
        public bool GeneratedInIncorrectRegion { get; set; } = false;
        public string IncompleteReason { get; set; } = "";
        public bool IncompleteServiceDescription { get; set; } = false;
        public bool IncorrectSort { get; set; } = false;
        public string InfoConsultants { get; set; } = "";
       // public string InterimClearance { get; set; } = "";
        // "InterimClearanceDateAndTime": null,
        public string ManualUrgency { get; set; } = "";
        public string ManualUrgencyComments { get; set; } = "";
        public bool Others { get; set; } = false;
        public string PaceRegion { get; set; }
        public string PreResearchFailed { get; set; } = "";
        public string PreScreeningFailed { get; set; } = "";
        public string PrimarySort { get; set; } = "";
        public string ReasonFor0Entity { get; set; } = "";
        public string ReasonsForAbortingACheck { get; set; } = "";
        public string ReasonsForUnableToComplete { get; set; } = "";
        public string Region { get; set; }
        public string RegionalNuance { get; set; } = "";
        public string RegionalNuanceComments { get; set; } = "";
        public string ResearchFilePath { get; set; } = "";
        public string ResubmissionComments { get; set; } = "";
        public string ResubmissionReasonS { get; set; } = "";
        public string ReturnToTeamComments { get; set; } = "";
        public string ReviewerName { get; set; } = "";
        public string ReviewRequired { get; set; } = "";
        public string ServiceCode { get; set; } = "";
        public bool ServiceDescriptionInOtherLanguage { get; set; } = false;
        public string ServiceLine { get; set; } = "";
        public string SortService { get; set; } = "";
        public DateTime SubmittedDateTime { get; set; }
        public string SubServiceLine { get; set; } = "";
        public string UrgentCheck { get; set; } = "";
     // public bool IsErrored { get; set; }
     // public string ErrorMessage { get; set; }
        //"OnHoldReason": [],
        //"ReviewChecklist": [],
        //"QuerySpocs": [],
        //"ReworkReason": []
    }

    public class CheckProcess
    {
        public static string NormalCheck = "Normal";
        public static string G360NormalCheck = "G360"; 
        public static string ResubmittedNormalCheck = "Resubmitted";
        public static string MultiEntityCheck = "Multi Entity Manual";
        public static string NormalCheckManual = "Normal Check - Manual";
    }
    public class ProcessNames
    {
        public static string NormalCheck = "Normal Check";
        public static string G360NormalCheck = "G360 - Normal Check";
        public static string ResubmittedNormalCheck = "Resubmitted - Normal Check";
        public static string MultiEntityCheck = "Multi Entity Check";
        public static string NormalCheckManual = "Normal Check - Manual";
    }
    public class CaseCreation
    {
        public CheckInfo checkInfo { get; set; }
        public string processNames { get; set; }
    }
    public class AreaRegion
    {
        public string Area;
      //  public string Region;
        public string CountryCode;
    }

    public class Action_Get_DataFields
    {
        public string AbortReason { get; set; }
        public int ActualEntitySCount { get; set; }
        public string AdditionalSort { get; set; }
        public string BasedOnCompletedPursuitCheck { get; set; }
        public string CheckId { get; set; }
        public string ChecklistForCannedText { get; set; }
        public string CheckProcess { get; set; }
        public string CheckType { get; set; }
    //    public string ClientSideCheckOrNoRelationshipWithAnyOfTheCp { get; set; }
    //    public string ClientSideCheckOrNoRelationshipWithAnyOfTheCpSManual { get; set; }
        public string CommentsForUnableToComplete { get; set; }
        public bool CompetitiveSituation { get; set; }
        public string Confidentiality { get; set; }
        public bool ConflictConditions { get; set; }
        public bool ConflictOfInterest { get; set; }
        public string ConsolidationFailed { get; set; }
        public bool CounterPartiesMissingNotProvidedInPace { get; set; }
        public bool CounterpartiesNotAddedInPaceApg { get; set; }
        public string Country { get; set; }
     //   public bool CpReworkRequired { get; set; }
        public string EntityName { get; set; }
        public int EntitySBot { get; set; }
        public int EntitySPaceApg { get; set; }
        public string G360 { get; set; }
        public string GcspConsultants { get; set; }
        public bool GeneratedInIncorrectRegion { get; set; }
        public string IncompleteReason { get; set; }
        public bool IncompleteServiceDescription { get; set; }
        public bool IncorrectSort { get; set; }
        public string InfoConsultants { get; set; }
    //    public string InterimClearance { get; set; }
     //   public object InterimClearanceDateAndTime { get; set; }
        public string ManualUrgency { get; set; }
        public string ManualUrgencyComments { get; set; }
        public bool Others { get; set; }
        public string PaceRegion { get; set; }
        public string PreResearchFailed { get; set; }
        public string PreScreeningFailed { get; set; }
        public string PrimarySort { get; set; }
        public string ReasonFor0Entity { get; set; }
        public string ReasonsForAbortingACheck { get; set; }
        public string ReasonsForUnableToComplete { get; set; }
        public string Region { get; set; }
        public string RegionalNuance { get; set; }
        public string RegionalNuanceComments { get; set; }
        public string ResearchFilePath { get; set; }
        public string ResubmissionComments { get; set; }
        public string ResubmissionReasonS { get; set; }
        public string ResubmittedCheck { get; set; }
        public string ReturnToTeamComments { get; set; }
        public string ReviewerName { get; set; }
        public string ReviewerNameCaseNumber { get; set; }
        public string ReviewRequired { get; set; }
        public string ServiceCode { get; set; }
        public bool ServiceDescriptionInOtherLanguage { get; set; }
        public string ServiceLine { get; set; }
        public string SortService { get; set; }
        public DateTime? SubmittedDateTime { get; set; }
        public string SubServiceLine { get; set; }
        public string UrgentCheck { get; set; }
        public List<object> OnHoldReason { get; set; }
        public List<object> ReviewChecklist { get; set; }
     //   public List<object> QuerySpocs { get; set; }
        public List<object> ReworkReason { get; set; }
    }


}
