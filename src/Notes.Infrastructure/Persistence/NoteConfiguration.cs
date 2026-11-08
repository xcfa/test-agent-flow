using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notes.Domain;

namespace Notes.Infrastructure.Persistence;

internal sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Notes");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n => n.Title).IsRequired().HasMaxLength(Note.MaxTitleLength);
        builder.Property(n => n.Body).IsRequired();
        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.UpdatedAt).IsRequired();
        builder.HasIndex(n => n.UpdatedAt);
    }
}
