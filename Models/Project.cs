namespace ProjectBoard.Models;

/// <summary>A data model class representing a project listing.</summary>
class Project {
    /// <summary>A unique identifier for the project (used as database primary key).</summary>
    public int Id { get; set; }
    /// <summary>The display title for the project.</summary>
    public string Title { get; set; } = "Untitled Project";
    /// <summary>The long body description for the project (formatted in markdown).</summary>
    public string? Description { get; set; } = null;
    /// <summary>The short project summary (a sentence or two).</summary>
    public string? Summary { get; set; } = null;
    /// <summary>The URL to the main cover image for the project.</summary>
    public string? CoverImageURL { get; set; } = null;
    /// <summary>The email address to contact for information about the project.</summary>
    public string? ContactEmail { get; set; } = null;

    // In the future we will incude more details about the creator/poster and the team, etc.
    //! For now, I also have the Id automaticly generated, but I might wish to update this.
    //! I might want to add default values or optionals for some properties.

//     public Project(int id = -1, string? title = "Untitled Project", string? description = null, 
//         string? summary = null, string? coverImageURL = null) {
//         this.Title = title;
//         this.Description = description;
//         this.Summary = summary;
//         this.CoverImageURL = coverImageURL;

//         // Deal with ID.
//     }
}