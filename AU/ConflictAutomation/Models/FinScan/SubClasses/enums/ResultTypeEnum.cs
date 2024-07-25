namespace ConflictAutomation.Models.FinScan.SubClasses.enums;

public enum ResultTypeEnum
{
    // 0 No Matches Found or all Matches are set as Safe or False Positive.
    PASS,

    // 1 One or more Matches currently in a Review Status (no Confirmed Hits).
    PENDING,

    // 2 One or more Matches currently in a Confirmed Hit Status
    FAIL,

    // 3 An error occurrend during processing. Additional information available in Messsage field. Please verify the data and resubmit.
    ERROR,

    // 4 This Status indicates an unexpected error that was not handled by the Wrapper. Please inform the Customer Success Team if you receive this value in a response.
    UNINITIALIZED,

    // 5 One or more Matches currently in a Duplicate Status (no In Review or Confirmed Hits) - requires FinScan Premium.
    DUPLICATE
}
