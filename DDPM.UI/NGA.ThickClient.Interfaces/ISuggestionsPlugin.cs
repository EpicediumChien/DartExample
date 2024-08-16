#region LicenceHeader

//
// Copyright © 2023, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.ThickClient.Interfaces;

/// <summary>
///
/// </summary>
public interface ISuggestionPlugin
{
    /// <summary>
    /// This method adds <see cref="SuggestionModel"/> to SuggestionPlugin
    /// </summary>
    /// <param name="suggestionModel"></param>
    /// <param name="position"></param>
    /// <returns>True if <paramref name="suggestionModel"/> is added successfully</returns>
    /// <exception cref="InvalidOperationException">This exception is thrown if the suggestion plugin is not in running condition</exception>
    Task<bool> AddSuggestionAsync(SuggestionModel suggestionModel, int position);

    /// <summary>
    /// This method removes <see cref="SuggestionModel"/> from SuggestionPlugin
    /// </summary>
    /// <param name="suggestionModel"></param>
    /// <returns>True if <paramref name="suggestionModel"/> was removed successfully</returns>
    /// <exception cref="InvalidOperationException">This exception is thrown if the Suggestion plugin is not in running condition</exception>
    Task<bool> RemoveSuggestionAsync(SuggestionModel suggestionModel);

    /// <summary>
    /// This method checks if the <see cref="SuggestionModel"/> exists in SuggestionPlugin
    /// </summary>
    /// <param name="suggestionModel"></param>
    /// <returns>True is returned if <paramref name="suggestionModel"/> exists</returns>
    /// <exception cref="InvalidOperationException">This exception is thrown if the suggestion plugin is not in running condition</exception>
    Task<bool> SuggestionExistsAsync(SuggestionModel suggestionModel);
}