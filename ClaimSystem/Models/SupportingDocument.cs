using System;
using System.Collections.Generic;

namespace ClaimSystem.Models;

public partial class SupportingDocument
{
    public int SupportingDocumentId { get; set; }

    public int ClaimId { get; set; }

    public byte[] FileContent { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = null!;

    public DateTime FileUploadDate { get; set; }

    public string FilePath { get; set; } = null!;

    public string? Description { get; internal set; }

    public virtual Claim Claim { get; set; } = null!;
    
}
