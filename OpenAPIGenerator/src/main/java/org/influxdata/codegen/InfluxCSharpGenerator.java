package org.influxdata.codegen;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
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
import org.openapitools.codegen.CodegenParameter;
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
        if (name.equals("TelegrafRequestPlugin")) {

            codegenModel.interfaces.clear();
            codegenModel.readWriteVars.clear();

            //
            // Add generic name
            //
            CodegenProperty nameProperty = new CodegenProperty();
            nameProperty.name = "name";
            nameProperty.baseName = "name";
            nameProperty.getter = "getName";
            nameProperty.setter = "setName";
            nameProperty.dataType = "N";
            nameProperty.datatypeWithEnum = "N";
            nameProperty.nameInSnakeCase = "NAME";
            nameProperty.isReadOnly = false;
            nameProperty.hasMore = true;
            nameProperty.hasMoreNonReadOnly = true;
            nameProperty.isPrimitiveType = false;
            postProcessModelProperty(codegenModel, nameProperty);
            codegenModel.vars.add(nameProperty);
            codegenModel.readWriteVars.add(nameProperty);

            //
            // Add type
            //
            CodegenProperty typeProperty = new CodegenProperty();
            typeProperty.name = "type";
            typeProperty.baseName = "type";
            typeProperty.getter = "getType";
            typeProperty.setter = "setType";
            typeProperty.dataType = "String";
            typeProperty.isEnum = true;
            typeProperty.set_enum(Arrays.asList("input", "output"));

            final HashMap<String, Object> allowableValues = new HashMap<>();

            List<Map<String, String>> enumVars = new ArrayList<>();
            for (String value : Arrays.asList("input", "output")) {
                Map<String, String> enumVar = new HashMap<>();
                enumVar.put("name", value.toUpperCase());
                enumVar.put("value", "" + value + "");
                enumVars.add(enumVar);
            }
            allowableValues.put("enumVars", enumVars);

            typeProperty.setAllowableValues(allowableValues);
            typeProperty.datatypeWithEnum = "TypeEnum";
            typeProperty.nameInSnakeCase = "TYPE";
            typeProperty.isReadOnly = false;
            typeProperty.hasMore = true;
            typeProperty.hasMoreNonReadOnly = true;
            postProcessModelProperty(codegenModel, typeProperty);
            codegenModel.vars.add(typeProperty);
            codegenModel.readWriteVars.add(typeProperty);

            //
            // Add generic config
            //
            CodegenProperty configProperty = new CodegenProperty();
            configProperty.name = "config";
            configProperty.baseName = "config";
            configProperty.getter = "getConfig";
            configProperty.setter = "setConfig";
            configProperty.dataType = "C";
            configProperty.datatypeWithEnum = "C";
            configProperty.nameInSnakeCase = "CONFIG";
            configProperty.isReadOnly = false;
            configProperty.isPrimitiveType = false;
            postProcessModelProperty(codegenModel, configProperty);
            codegenModel.vars.add(configProperty);
            codegenModel.readWriteVars.add(configProperty);

            //
            // Add generics to class
            //
            codegenModel.vendorExtensions.put("x-has-generic-type", Boolean.TRUE);
            codegenModel.vendorExtensions.put("x-generic-type", "<N, C>");
        }

        codegenModel.allVars.stream().filter(property -> "TelegrafRequestPlugin".equals(property.complexType) && property.isContainer)
                .forEach(property -> {
                    property.dataType = "List<TelegrafRequestPlugin<object,object>>";
                    property.datatypeWithEnum = property.dataType;
                });


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

        //
        // Set base path
        //
        String url;
        if (operation.getServers() != null) {
            url = operation.getServers().get(0).getUrl();
        } else if (openAPI.getPaths().get(path).getServers() != null) {
            url = openAPI.getPaths().get(path).getServers().get(0).getUrl();
        } else {
            url = openAPI.getServers().get(0).getUrl();
        }

        if (!url.equals("/")) {
            codegenOperation.path = url + codegenOperation.path;
        }

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

    @Override
    public Map<String, Object> postProcessOperationsWithModels(final Map<String, Object> objs,
                                                               final List<Object> allModels) {

        Map<String, Object> operationsWithModels = super.postProcessOperationsWithModels(objs, allModels);

        List<CodegenOperation> operations = (List<CodegenOperation>) ((HashMap) operationsWithModels.get("operations"))
                .get("operation");

        //
        // For basic auth add authorization header
        //
        operations.stream()
                .filter(operation -> operation.hasAuthMethods)
                .forEach(operation -> {

                    operation.authMethods.stream()
                            .filter(security -> security.isBasic)
                            .forEach(security -> {

                                CodegenParameter authorization = new CodegenParameter();
                                authorization.isHeaderParam = true;
                                authorization.isPrimitiveType = true;
                                authorization.isString = true;
                                authorization.baseName = "Authorization";
                                authorization.paramName = "authorization";
                                authorization.dataType = "String";
                                authorization.description = "An auth credential for the Basic scheme";

                                operation.allParams.get(operation.allParams.size() - 1).hasMore = true;
                                operation.allParams.add(authorization);
                                
                                operation.headerParams.get(operation.headerParams.size() - 1).hasMore = true;
                                operation.headerParams.add(authorization);
                            });
                });

        return operationsWithModels;

    }

    @Override
    public String toApiName(String name) {

        if (name.length() == 0) {
            return "DefaultService";
        }

        //
        // Rename "Api" to "Service"
        //
        return initialCaps(name) + "Service";
    }
}