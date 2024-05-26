namespace ProjectBoard.Models;

/// <summary>A data model class representing a project listing.</summary>
class Project {
    /// <summary>A unique identifier for the project (used as database primary key).</summary>
    public int Id { get; set; }
    /// <summary>The display title for the project.</summary>
    public string Title { get; set; }
    /// <summary>The long body description for the project (formatted in markdown).</summary>
    public string Description { get; set; }
    /// <summary>The short project summary (a sentence or two).</summary>
    public string Summary { get; set; }
    /// <summary>The URL to the main cover image for the project.</summary>
    public string CoverImageURL { get; set; }
    /// <summary>The email address to contact for information about the project.</summary>
    public string ContactEmail { get; set; }

    // In the future we will incude more details about the creator/poster and the team, etc. 
}