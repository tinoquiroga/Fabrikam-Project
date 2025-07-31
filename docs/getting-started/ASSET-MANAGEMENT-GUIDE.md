# Fabrikam API Asset Management Guide

## 📁 **Directory Structure**

```
FabrikamApi/src/wwwroot/assets/
├── products/                    # Product-related assets
│   ├── images/
│   │   ├── primary/            # Main product photos (hero images)
│   │   ├── gallery/            # Additional product photos
│   │   └── thumbnails/         # Optimized thumbnails (200x150px)
│   ├── blueprints/             # Architectural drawings (PDFs)
│   ├── brochures/              # Marketing materials (PDFs)
│   └── specifications/         # Technical specifications (PDFs)
├── categories/                  # Category images
└── branding/                   # Company logos and branding assets
```

## 🏷️ **Naming Conventions**

### **Pattern**: `{category}-{model-number}-{type}-{variant}.{extension}`

### **Product Images**
```
Primary Photos (Hero Images):
singlefamily-sf2400-main.jpg
duplex-dx1800-main.jpg
townhouse-th3200-main.jpg

Gallery Photos:
singlefamily-sf2400-exterior-front.jpg
singlefamily-sf2400-exterior-rear.jpg
singlefamily-sf2400-interior-kitchen.jpg
singlefamily-sf2400-interior-living.jpg
singlefamily-sf2400-interior-master.jpg

Thumbnails (Auto-generated):
singlefamily-sf2400-thumb.jpg
duplex-dx1800-thumb.jpg
```

### **Blueprints & Technical Documents**
```
Architectural Drawings:
singlefamily-sf2400-floorplan.pdf
singlefamily-sf2400-elevation.pdf
singlefamily-sf2400-foundation.pdf
singlefamily-sf2400-electrical.pdf
singlefamily-sf2400-plumbing.pdf

ADU Plans:
adu-terrazia-plans-A.pdf
adu-casita-plans-A.pdf
adu-studio-plans-B.pdf

Specifications:
singlefamily-sf2400-specs.pdf
singlefamily-sf2400-materials.pdf
singlefamily-sf2400-energy.pdf
```

### **Marketing Materials**
```
Brochures:
singlefamily-sf2400-brochure.pdf
duplex-dx1800-brochure.pdf
townhouse-collection2025-catalog.pdf
adu-terrazia-brochure.pdf
```

## 🎯 **Category Codes**

| Category | Code | Example |
|----------|------|---------|
| Single Family | `singlefamily` | `singlefamily-sf2400-main.jpg` |
| Duplex | `duplex` | `duplex-dx1800-main.jpg` |
| Townhouse | `townhouse` | `townhouse-th3200-main.jpg` |
| Apartment | `apartment` | `apartment-ap1200-main.jpg` |
| Commercial | `commercial` | `commercial-co5000-main.jpg` |
| Accessory Unit | `adu` | `adu-terrazia-plans-A.pdf` |
| Building Materials | `material` | `material-roofing-asphalt-main.jpg` |
| Windows & Doors | `windowsdoors` | `windowsdoors-vinyl-double-main.jpg` |
| HVAC Systems | `hvac` | `hvac-centralair-main.jpg` |

## 📏 **Image Specifications**

### **Primary Images (Hero)**
- **Resolution**: 1920x1080px (16:9 aspect ratio)
- **Format**: JPG (high quality, 85% compression)
- **Max Size**: 500KB
- **Use**: Product listings, featured displays

### **Gallery Images**
- **Resolution**: 1200x800px (3:2 aspect ratio)
- **Format**: JPG (80% compression)
- **Max Size**: 300KB
- **Use**: Product detail galleries

### **Thumbnails**
- **Resolution**: 300x200px (3:2 aspect ratio)
- **Format**: JPG (75% compression)
- **Max Size**: 50KB
- **Use**: Lists, cards, quick previews

### **Category Images**
- **Resolution**: 800x600px (4:3 aspect ratio)
- **Format**: JPG (80% compression)
- **Max Size**: 200KB

## 📄 **PDF Specifications**

### **Blueprints**
- **Max Size**: 10MB per file
- **Resolution**: 300 DPI minimum
- **Format**: PDF/A (archival quality)
- **Security**: No restrictions for viewing

### **Brochures & Marketing**
- **Max Size**: 5MB per file
- **Resolution**: 150-300 DPI
- **Format**: PDF with embedded fonts
- **Optimization**: Web-optimized for fast loading

## 🔗 **API URL Patterns**

### **Static File URLs**
```
Primary Image:
https://fabrikam-api-dev-izbd.azurewebsites.net/assets/products/images/primary/singlefamily-sf2400-main.jpg

Gallery Images:
https://fabrikam-api-dev-izbd.azurewebsites.net/assets/products/images/gallery/singlefamily-sf2400-exterior-front.jpg

Blueprints:
https://fabrikam-api-dev-izbd.azurewebsites.net/assets/products/blueprints/singlefamily-sf2400-floorplan.pdf

ADU Plans:
https://fabrikam-api-dev-izbd.azurewebsites.net/assets/products/blueprints/adu-terrazia-plans-A.pdf

Brochures:
https://fabrikam-api-dev-izbd.azurewebsites.net/assets/products/brochures/singlefamily-sf2400-brochure.pdf
```

### **API Endpoints for Assets**
```
Get Product Images:
GET /api/products/{id}/images

Get Product Blueprints:
GET /api/products/{id}/blueprints

Get Product Documents:
GET /api/products/{id}/documents
```

## 🏗️ **Database Integration**

### **Product Model Extensions**
Add these properties to your Product model:

```csharp
public class Product
{
    // ... existing properties ...
    
    // Asset URLs
    public string? PrimaryImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    
    // Asset collections
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<ProductDocument> Documents { get; set; } = new List<ProductDocument>();
}

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ImageType { get; set; } = string.Empty; // "primary", "gallery", "thumbnail"
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
    public virtual Product Product { get; set; } = null!;
}

public class ProductDocument
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string DocumentUrl { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty; // "blueprint", "brochure", "specification"
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long FileSizeBytes { get; set; }
    public virtual Product Product { get; set; } = null!;
}
```

## 📝 **Example File Naming**

### **Single Family Homes**
```
Images:
- singlefamily-sf2400-main.jpg
- singlefamily-sf2400-exterior-front.jpg
- singlefamily-sf2400-interior-kitchen.jpg
- singlefamily-sf2400-thumb.jpg

Blueprints:
- singlefamily-sf2400-floorplan.pdf
- singlefamily-sf2400-elevation.pdf

Marketing:
- singlefamily-sf2400-brochure.pdf
- singlefamily-sf2400-specs.pdf
```

### **Accessory Dwelling Units (ADU)**
```
Images:
- adu-terrazia-main.jpg
- adu-terrazia-exterior-front.jpg
- adu-terrazia-interior-living.jpg
- adu-terrazia-thumb.jpg

Plans:
- adu-terrazia-plans-A.pdf
- adu-casita-plans-A.pdf
- adu-studio-plans-B.pdf

Marketing:
- adu-terrazia-brochure.pdf
- adu-collection2025-catalog.pdf
```

### **Building Materials**
```
Images:
- material-siding-vinyl-white-main.jpg
- material-roofing-metal-standingseam-main.jpg
- material-flooring-hardwood-oak-main.jpg

Specifications:
- material-siding-vinyl-white-specs.pdf
- material-roofing-metal-installation.pdf
```

## 🚀 **Implementation Steps**

1. **Add asset properties to Product model**
2. **Create ProductImage and ProductDocument entities**
3. **Update ProductsController to include asset URLs**
4. **Add file upload endpoints for asset management**
5. **Update ProductsController to serve asset information**
6. **Configure static file serving in Program.cs**

## 🔒 **Security Considerations**

- Validate file types on upload
- Scan files for malware
- Implement file size limits
- Use CDN for better performance
- Consider Azure Blob Storage for production
- Implement proper access controls for sensitive blueprints

---
*Asset Management Guide for Fabrikam Modular Homes API*  
*Last Updated: July 2025*
