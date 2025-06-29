﻿using SecuritySystem.Builders._Filter;

namespace SecuritySystem.Builders._Factory;

public interface ISecurityFilterFactory<TDomainObject> : IFilterFactory<TDomainObject, SecurityFilterInfo<TDomainObject>>;
