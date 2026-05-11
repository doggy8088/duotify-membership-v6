using System.Collections.Concurrent;

namespace Duotify.Membership.Web.Infrastructure.Email;

public sealed class VerificationEmailPreviewStore
{
    private readonly ConcurrentDictionary<string, VerificationEmailPreview> _previews = new(StringComparer.Ordinal);

    public VerificationEmailPreview? Get(string registrationReference)
    {
        if (string.IsNullOrWhiteSpace(registrationReference))
        {
            return null;
        }

        return _previews.TryGetValue(registrationReference, out var preview) ? preview : null;
    }

    public void Save(string registrationReference, VerificationEmailPreview preview)
    {
#if RELEASE
        return;
#else
        if (string.IsNullOrWhiteSpace(registrationReference))
        {
            return;
        }

        _previews[registrationReference] = preview;
#endif
    }

    public void Clear(string registrationReference)
    {
        if (string.IsNullOrWhiteSpace(registrationReference))
        {
            return;
        }

        _previews.TryRemove(registrationReference, out _);
    }
}