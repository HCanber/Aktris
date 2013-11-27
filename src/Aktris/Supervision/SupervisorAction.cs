namespace Aktris.Supervision
{
	public enum SupervisorAction
	{
		/// <summary>Resumes message processing for the failed Actor</summary>
		Resume,

		/// <summary>Discards the old Actor instance and replaces it with a new, then resumes message processing.</summary>
		Restart,

		/// <summary>Stops the Actor</summary>
		Stop,

		/// <summary>
		/// Escalates the failure to the supervisor of the supervisor, 
		/// by rethrowing the cause of the failure, i.e. the supervisor fails with
		/// the same exception as the child.
		/// </summary>
		Escalate,
	}
}