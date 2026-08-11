# 15 - Master Dependency

## Ajay Industries ERP

This document describes dependencies between Master modules.

---

# 1. Dependency Principle

A Master should depend only on data that is required for its stable identity or business use.

Transaction-derived information should not be stored in Master tables.

---

# 2. Master Dependency Overview

```text
Company
Employee
UOM
Warehouse
Item Category
Brand
Shape
Specification
        ↓
       Item
        ↓
      Drawing

Supplier