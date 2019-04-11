package org.influxdata.codegen;

import java.util.Arrays;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.stream.Collectors;
import javax.annotation.Nonnull;

import io.swagger.v3.oas.models.OpenAPI;
import io.swagger.v3.oas.models.Operation;
import io.swagger.v3.oas.models.media.Schema;
import org.openapitools.codegen.CodegenModel;
import org.openapitools.codegen.CodegenOperation;
import org.openapitools.codegen.CodegenProperty;
import org.openapitools.codegen.languages.CSharpClientCodegen;

public class InfluxCSharpGenerator extends CSharpClientCodegen {

    /**
     * Configures a friendly name for the generator.  This will be used by the generator
     * to select the library with the -g flag.
     *
     * @return the friendly name for the generator
     */
    @Nonnull 
    public String getName() {
        return "influx-csharp";
    }

    /**
     * Returns human-friendly help for the generator.  Provide the consumer with help
     * tips, parameters here
     *
     * @return A string value for the help message
     */
    @Nonnull 
    public String getHelp() {
        return "Generates a influx-csharp client library.";
    }

    public InfluxCSharpGenerator() {
        super();

        embeddedTemplateDir = templateDir = "csharp";
    }

    @Override
    public CodegenModel fromModel(final String name, final Schema model, final Map<String, Schema> allDefinitions) {
        CodegenModel codegenModel = super.fromModel(name, model, allDefinitions);

        //
        // set default enum value
        //
        codegenModel.getAllVars().stream()
                .filter(property -> property.isEnum)
                .forEach(this::updateCodegenPropertyEnum);

        //
        // Remove parent read only vars
        //
        Set<CodegenProperty> readOnly = codegenModel.getParentVars().stream()
                .filter(property -> property.isReadOnly)
                .collect(Collectors.toSet());
        codegenModel.getParentVars().removeAll(readOnly);

        //
        // Add generic name, type and config property
        //
//        if (name.equals("TelegrafRequestPlugin")) {
//
//            codegenModel.interfaces.clear();
//            codegenModel.setDataType("TelegrafRequestPlugin<T, C>");
//
//            //
//            // Add generic name
//            //
//            CodegenProperty nameProperty = new CodegenProperty();
//            nameProperty.name = "name";
//            nameProperty.baseName = "name";
//            nameProperty.getter = "getName";
//            nameProperty.setter = "setName";
//            nameProperty.dataType = "T";
//            nameProperty.datatypeWithEnum = "T";
//            nameProperty.nameInSnakeCase = "NAME";
//            nameProperty.isReadOnly = false;
//            nameProperty.hasMore = true;
//            nameProperty.hasMoreNonReadOnly = true;
//            postProcessModelProperty(codegenModel, nameProperty);
//            codegenModel.vars.add(nameProperty);
//            codegenModel.readWriteVars.add(nameProperty);
//
//            //
//            // Add type
//            //
//            CodegenProperty typeProperty = new CodegenProperty();
//            typeProperty.name = "type";
//            typeProperty.baseName = "type";
//            typeProperty.getter = "getType";
//            typeProperty.setter = "setType";
//            typeProperty.dataType = "String";
//            typeProperty.isEnum = true;
//            typeProperty.set_enum(Arrays.asList("input", "output"));
//
//            final HashMap<String, Object> allowableValues = new HashMap<>();
//
//            List<Map<String, String>> enumVars = new ArrayList<>();
//            for (String value : Arrays.asList("input", "output")) {
//                Map<String, String> enumVar = new HashMap<>();
//                enumVar.put("name", value.toUpperCase());
//                enumVar.put("value", "" + value + "");
//                enumVars.add(enumVar);
//            }
//            allowableValues.put("enumVars", enumVars);
//
//            typeProperty.setAllowableValues(allowableValues);
//            typeProperty.datatypeWithEnum = "TypeEnum";
//            typeProperty.nameInSnakeCase = "TYPE";
//            typeProperty.isReadOnly = false;
//            typeProperty.hasMore = true;
//            typeProperty.hasMoreNonReadOnly = true;
//            postProcessModelProperty(codegenModel, typeProperty);
//            codegenModel.vars.add(typeProperty);
//            codegenModel.readWriteVars.add(typeProperty);
//
//            //
//            // Add generic config
//            //
//            CodegenProperty configProperty = new CodegenProperty();
//            configProperty.name = "config";
//            configProperty.baseName = "config";
//            configProperty.getter = "getConfig";
//            configProperty.setter = "setConfig";
//            configProperty.dataType = "C";
//            configProperty.datatypeWithEnum = "C";
//            configProperty.nameInSnakeCase = "CONFIG";
//            configProperty.isReadOnly = false;
//            postProcessModelProperty(codegenModel, configProperty);
//            codegenModel.vars.add(configProperty);
//            codegenModel.readWriteVars.add(configProperty);
//
//            //
//            // Add generics to class
//            //
//            codegenModel.vendorExtensions.put("x-has-generic-type", Boolean.TRUE);
//            codegenModel.vendorExtensions.put("x-generic-type", "<T, C>");
//        }

        return codegenModel;
    }

    @Override
    public CodegenOperation fromOperation(final String path, final String httpMethod, final Operation operation, final Map<String, Schema> schemas, final OpenAPI openAPI) {

        CodegenOperation codegenOperation = super.fromOperation(path, httpMethod, operation, schemas, openAPI);

        //
        // Add optional for enum that doesn't have a default value
        //
        codegenOperation.allParams.stream()
                .filter(parameter -> parameter.defaultValue == null && !parameter.required && schemas.containsKey(parameter.dataType))
                .filter(parameter -> {
                    List enums = schemas.get(parameter.dataType).getEnum();
                    return enums != null && !enums.isEmpty();
                })
                .forEach(op -> op.dataType += "?");

        return codegenOperation;
    }

    @Override
    public void processOpts() {

        super.processOpts();

        List<String> accepted = Arrays.asList(
                "ApiResponse.cs", "OpenAPIDateConverter.cs", "ExceptionFactory.cs",
                "Configuration.cs", "ApiException.cs", "IApiAccessor.cs", "ApiClient.cs",
                "IReadableConfiguration.cs", "GlobalConfiguration.cs");

        //
        // We want to use only the JSON.java
        //
        supportingFiles = supportingFiles.stream()
                .filter(supportingFile -> accepted.contains(supportingFile.destinationFilename))
                .collect(Collectors.toList());
    }
}