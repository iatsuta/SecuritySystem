﻿namespace SecuritySystem.AncestorDenormalization;

public interface IDenormalizedAncestorsService<in TDomainObject, in TDomainObjectAncestorLink> : IDenormalizedAncestorsService<TDomainObject>;