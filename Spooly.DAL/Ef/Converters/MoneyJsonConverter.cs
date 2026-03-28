using System.Text.Json;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Spooly.Models;

namespace Spooly.DAL.Ef.Converters;

internal sealed class MoneyJsonConverter() : ValueConverter<Money?, string?>(
  v => v.HasValue ? JsonSerializer.Serialize(v.Value, (JsonSerializerOptions?)null) : null,
	v => string.IsNullOrWhiteSpace(v) ? null : JsonSerializer.Deserialize<Money>(v, (JsonSerializerOptions?)null))
{
}
