using ConflictAutomation.Extensions;
using ConflictAutomation.Models.PreScreening.SubClasses;

namespace ConflictAutomation.Mappers;

public class TeamMemberMapper
{
    private static TeamMember CreateFrom(PACE.TeamMember paceTeamMember) => new()
    {
        Role = paceTeamMember.RoleName,
        Name = paceTeamMember.FullName, 
        Email = (string.IsNullOrWhiteSpace(paceTeamMember.Email) 
                    ? string.Empty : paceTeamMember.Email),
        Preparer = paceTeamMember.Preparer,
        DateAdded = (paceTeamMember.CreateDateTime > DateTime.MinValue) 
                        ? paceTeamMember.CreateDateTime.TimestampWithTimezoneFromUtc()
                        : string.Empty
    };


    private static TeamMember CreateFrom(PACE.ConflictCheckTeamMember paceConflictCheckTeamMember) => new()
    {
        Role = paceConflictCheckTeamMember.RoleName,
        Name = paceConflictCheckTeamMember.MemberName, 
        Email = (string.IsNullOrWhiteSpace(paceConflictCheckTeamMember.MemberEmail) 
                    ? string.Empty : paceConflictCheckTeamMember.MemberEmail),
        Preparer = paceConflictCheckTeamMember.Preparer,
        DateAdded = (paceConflictCheckTeamMember.CreateDateTime > DateTime.MinValue)
                        ? paceConflictCheckTeamMember.CreateDateTime.TimestampWithTimezoneFromUtc()
                        : string.Empty
    };


    private static List<TeamMember> CreateFrom(List<PACE.TeamMember> paceTeamMembers) =>
        paceTeamMembers.IsNullOrEmpty() ? [] : paceTeamMembers.Select(CreateFrom).ToList();


    private static List<TeamMember> CreateFrom(List<PACE.ConflictCheckTeamMember> paceConflictCheckTeamMembers) =>
        paceConflictCheckTeamMembers.IsNullOrEmpty() ? [] : paceConflictCheckTeamMembers.Select(CreateFrom).ToList();


    public static List<TeamMember> CreateFrom(List<PACE.TeamMember> paceTeamMembers,
                                              List<PACE.ConflictCheckTeamMember> paceConflictCheckTeamMembers) =>
        CreateFrom(paceTeamMembers)
            .Concat(CreateFrom(paceConflictCheckTeamMembers))
            .Distinct()
            .ToList();
}
